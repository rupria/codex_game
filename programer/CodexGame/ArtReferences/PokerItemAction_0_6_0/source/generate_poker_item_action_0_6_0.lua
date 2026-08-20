-- Text-safe poker item action buttons for GitHub issue #71.
-- No label or horizontal rule is baked into the runtime sprites.
-- Required params: runtimeRoot, sourceRoot, previewRoot

local p=app.params
local runtimeRoot=assert(p.runtimeRoot,"runtimeRoot is required")
local sourceRoot=assert(p.sourceRoot,"sourceRoot is required")
local previewRoot=assert(p.previewRoot,"previewRoot is required")

local function image(w,h)
  local im=Image(w,h,ColorMode.RGB); im:clear(Color{r=0,g=0,b=0,a=0}); return im
end
local function fill(im,x,y,w,h,c)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function lineH(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function lineV(im,x,y,h,c,t) fill(im,x,y,t or 1,h,c) end
local function frame(im,x,y,w,h,c,t)
  t=t or 1; lineH(im,x,y,w,c,t); lineH(im,x,y+h-t,w,c,t); lineV(im,x,y,h,c,t); lineV(im,x+w-t,y,h,c,t)
end
local function diamond(im,cx,cy,r,c)
  for y=-r,r do local w=r-math.abs(y); lineH(im,cx-w,cy+y,w*2+1,c,1) end
end

local C={
  ink=Color{r=8,g=6,b=5,a=255}, shadow=Color{r=0,g=0,b=0,a=170},
  leather=Color{r=24,g=16,b=12,a=255}, leather2=Color{r=38,g=24,b=16,a=255},
  brass0=Color{r=72,g=42,b=17,a=255}, brass1=Color{r=130,g=78,b=26,a=255},
  brass2=Color{r=201,g=137,b=48,a=255}, brass3=Color{r=242,g=191,b=82,a=255},
  disabled=Color{r=67,g=61,b=53,a=255}, text=Color{r=238,g=223,b=185,a=255}
}

local function button(state)
  local im=image(172,44)
  local border=state=="hover" and C.brass3 or state=="disabled" and C.disabled or C.brass1
  local face=state=="disabled" and Color{r=23,g=22,b=20,a=255} or C.leather
  fill(im,4,5,164,36,C.shadow)
  fill(im,1,2,170,38,C.ink); fill(im,4,5,164,32,border); fill(im,7,8,158,26,face)
  frame(im,9,10,154,22,state=="hover" and C.brass2 or C.brass0,1)
  diamond(im,12,21,3,state=="disabled" and C.disabled or C.brass2)
  diamond(im,159,21,3,state=="disabled" and C.disabled or C.brass2)
  lineH(im,22,11,128,state=="hover" and C.brass1 or C.leather2,1)
  lineH(im,22,31,128,C.ink,1)
  return im
end

local states={"idle","hover","disabled"}; local generated={}
for _,state in ipairs(states) do
  generated[state]=button(state)
  generated[state]:saveAs(runtimeRoot.."/poker_item_action_button_"..state.."_172x44_0_6_0.png")
end

local glyph={
  A={"01110","10001","10001","11111","10001","10001","10001"},
  B={"11110","10001","10001","11110","10001","10001","11110"},
  C={"01111","10000","10000","10000","10000","10000","01111"},
  D={"11110","10001","10001","10001","10001","10001","11110"},
  E={"11111","10000","10000","11110","10000","10000","11111"},
  F={"11111","10000","10000","11110","10000","10000","10000"},
  I={"11111","00100","00100","00100","00100","00100","11111"},
  M={"10001","11011","10101","10101","10001","10001","10001"},
  N={"10001","11001","10101","10011","10001","10001","10001"},
  O={"01110","10001","10001","10001","10001","10001","01110"},
  R={"11110","10001","10001","11110","10100","10010","10001"},
  S={"01111","10000","10000","01110","00001","00001","11110"},
  U={"10001","10001","10001","10001","10001","10001","01110"}
}
local function label(im,text,x,y,scale,color)
  local cursor=x
  for ch in text:gmatch(".") do
    local rows=glyph[ch]
    if rows then
      for yy,row in ipairs(rows) do
        for xx=1,5 do if row:sub(xx,xx)=="1" then fill(im,cursor+(xx-1)*scale,y+(yy-1)*scale,scale,scale,color) end end
      end
      cursor=cursor+6*scale
    else cursor=cursor+4*scale end
  end
end

local preview=image(960,300); fill(preview,0,0,960,300,Color{r=12,g=9,b=7,a=255})
local names={"USE","CONFIRM","DISABLED"}
for i,state in ipairs(states) do
  local x=110+(i-1)*270; local y=70
  preview:drawImage(generated[state],Point(x,y))
  local text=names[i]; local tw=#text*18-3
  label(preview,text,x+math.floor((172-tw)/2),y+15,3,state=="disabled" and C.disabled or C.text)
  preview:drawImage(generated[state],Point(x,160))
end
preview:saveAs(previewRoot.."/poker_item_action_button_states_960x300_0_6_0.png")

local src=Sprite(172,44,ColorMode.RGB); src.layers[1].name="poker_item_action_button_states"
src.cels[1].image:drawImage(generated.idle,Point(0,0))
for _,state in ipairs({"hover","disabled"}) do local f=src:newEmptyFrame(); src:newCel(src.layers[1],f,generated[state],Point(0,0)) end
src:saveAs(sourceRoot.."/poker_item_action_button_states_172x44_0_6_0.aseprite"); src:close()
