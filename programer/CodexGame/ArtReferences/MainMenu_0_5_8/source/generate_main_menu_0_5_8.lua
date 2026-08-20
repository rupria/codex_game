-- MainMenu 0.5.8 for GitHub issue #64.
-- Baseline: dev db443a7. The approved 2026-08-20 image is reference-only;
-- runtime labels stay live/localized and no 0.5.6 file is overwritten.

local p = app.params
local basePath = assert(p.base, "base is required")
local crestPath = assert(p.crest, "crest is required")
local runtimeRoot = assert(p.runtimeRoot, "runtimeRoot is required")
local sourceRoot = assert(p.sourceRoot, "sourceRoot is required")
local previewRoot = assert(p.previewRoot, "previewRoot is required")

local pc = app.pixelColor
local function img(w,h)
  local out = Image(w,h,ColorMode.RGB)
  out:clear(Color{r=0,g=0,b=0,a=0})
  return out
end
local function fill(im,x,y,w,h,c)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function lineH(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function lineV(im,x,y,h,c,t) fill(im,x,y,t or 1,h,c) end
local function frame(im,x,y,w,h,c,t)
  t=t or 1
  lineH(im,x,y,w,c,t); lineH(im,x,y+h-t,w,c,t)
  lineV(im,x,y,h,c,t); lineV(im,x+w-t,y,h,c,t)
end
local function poly(im,pts,c)
  local minY,maxY=pts[1][2],pts[1][2]
  for _,pt in ipairs(pts) do minY=math.min(minY,pt[2]); maxY=math.max(maxY,pt[2]) end
  for y=minY,maxY do
    local nodes={}; local j=#pts
    for i=1,#pts do
      local xi,yi=pts[i][1],pts[i][2]
      local xj,yj=pts[j][1],pts[j][2]
      if (yi<y and yj>=y) or (yj<y and yi>=y) then
        nodes[#nodes+1]=math.floor(xi+(y-yi)/(yj-yi)*(xj-xi))
      end
      j=i
    end
    table.sort(nodes)
    for i=1,#nodes-1,2 do lineH(im,nodes[i],y,nodes[i+1]-nodes[i]+1,c,1) end
  end
end
local function disc(im,cx,cy,r,c)
  for y=-r,r do
    local span=math.floor(math.sqrt(r*r-y*y))
    lineH(im,cx-span,cy+y,span*2+1,c,1)
  end
end
local function nearest(src,w,h)
  local out=img(w,h)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      out:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return out
end
local function loadImage(path)
  local spr=assert(app.open(path),"cannot open "..path)
  local out=spr.cels[1].image:clone()
  spr:close()
  return out
end
local function trimAlpha(src,threshold)
  local minX,minY,maxX,maxY=src.width,src.height,-1,-1
  for y=0,src.height-1 do
    for x=0,src.width-1 do
      local v=src:getPixel(x,y)
      if pc.rgbaA(v)>threshold then
        minX=math.min(minX,x); minY=math.min(minY,y)
        maxX=math.max(maxX,x); maxY=math.max(maxY,y)
      end
    end
  end
  assert(maxX>=minX and maxY>=minY,"crest has no visible pixels")
  local out=img(maxX-minX+1,maxY-minY+1)
  for y=minY,maxY do
    for x=minX,maxX do
      local v=src:getPixel(x,y)
      if pc.rgbaA(v)>threshold then out:drawPixel(x-minX,y-minY,v) end
    end
  end
  return out
end

local C={
  ink=Color{r=7,g=5,b=4,a=255}, shadow=Color{r=0,g=0,b=0,a=210},
  leather=Color{r=31,g=18,b=12,a=255}, leather2=Color{r=52,g=29,b=17,a=255},
  brass0=Color{r=74,g=40,b=13,a=255}, brass1=Color{r=137,g=79,b=22,a=255},
  brass2=Color{r=214,g=146,b=43,a=255}, brass3=Color{r=249,g=202,b=101,a=255},
  cyan=Color{r=43,g=213,b=211,a=255}, red=Color{r=226,g=52,b=48,a=255},
  cream=Color{r=247,g=226,b=174,a=255}
}
local function bolt(im,x,y)
  disc(im,x,y,4,C.ink); disc(im,x,y,3,C.brass1); fill(im,x,y-2,1,3,C.brass3)
end
local function button(state,accent)
  local out=img(380,84)
  local y=(state=="pressed") and 5 or 2
  fill(out,10,y+7,360,68,C.shadow)
  poly(out,{{6,y+12},{17,y+1},{363,y+1},{374,y+12},{374,y+66},{363,y+77},{17,y+77},{6,y+66}},C.ink)
  poly(out,{{10,y+13},{19,y+4},{361,y+4},{370,y+13},{370,y+65},{361,y+74},{19,y+74},{10,y+65}},C.brass1)
  poly(out,{{15,y+15},{23,y+7},{357,y+7},{365,y+15},{365,y+63},{357,y+71},{23,y+71},{15,y+63}},C.leather)
  frame(out,26,y+13,328,52,C.brass0,2)
  lineH(out,34,y+17,312,C.brass2,2)
  lineH(out,34,y+59,312,C.ink,2)
  poly(out,{{15,y+42},{25,y+32},{35,y+42},{25,y+52}},accent)
  poly(out,{{365,y+42},{355,y+32},{345,y+42},{355,y+52}},accent)
  bolt(out,22,y+12); bolt(out,358,y+12); bolt(out,22,y+66); bolt(out,358,y+66)
  if state=="hover" then
    lineH(out,42,y+10,296,C.brass3,2)
    lineH(out,55,y+15,270,accent,1)
  elseif state=="pressed" then
    fill(out,31,y+19,318,40,Color{r=12,g=8,b=6,a=105})
    lineH(out,50,y+61,280,C.brass2,2)
  end
  return out
end

local glyphs={
  A={"01110","10001","10001","11111","10001","10001","10001"},
  D={"11110","10001","10001","10001","10001","10001","11110"},
  E={"11111","10000","10000","11110","10000","10000","11111"},
  G={"01110","10001","10000","10111","10001","10001","01111"},
  I={"11111","00100","00100","00100","00100","00100","11111"},
  R={"11110","10001","10001","11110","10100","10010","10001"},
  S={"01111","10000","10000","01110","00001","00001","11110"},
  T={"11111","00100","00100","00100","00100","00100","00100"},
  U={"10001","10001","10001","10001","10001","10001","01110"}
}
local function text(im,word,cx,y,scale,c)
  local total=#word*6*scale-scale; local x0=math.floor(cx-total/2)
  for i=1,#word do
    local g=glyphs[word:sub(i,i)]
    if g then
      for yy,row in ipairs(g) do
        for xx=1,#row do
          if row:sub(xx,xx)=="1" then fill(im,x0+(i-1)*6*scale+(xx-1)*scale,y+(yy-1)*scale,scale,scale,c) end
        end
      end
    end
  end
end

local crestSource=trimAlpha(loadImage(crestPath),20)
local crest=nearest(crestSource,300,190)
local startIdle=button("idle",C.cyan)
local startHover=button("hover",C.cyan)
local startPressed=button("pressed",C.cyan)
local guideIdle=button("idle",C.red)
local guideHover=button("hover",C.red)
local guidePressed=button("pressed",C.red)

crest:saveAs(runtimeRoot.."/main_menu_duel_crest_300x190_0_5_8.png")
startIdle:saveAs(runtimeRoot.."/main_menu_start_idle_380x84_0_5_8.png")
startHover:saveAs(runtimeRoot.."/main_menu_start_hover_380x84_0_5_8.png")
startPressed:saveAs(runtimeRoot.."/main_menu_start_pressed_380x84_0_5_8.png")
guideIdle:saveAs(runtimeRoot.."/main_menu_guide_idle_380x84_0_5_8.png")
guideHover:saveAs(runtimeRoot.."/main_menu_guide_hover_380x84_0_5_8.png")
guidePressed:saveAs(runtimeRoot.."/main_menu_guide_pressed_380x84_0_5_8.png")

local base=loadImage(basePath)
local screen=base:clone()
screen:drawImage(crest,Point(330,60))
screen:drawImage(startIdle,Point(290,264))
screen:drawImage(guideIdle,Point(290,354))
screen:saveAs(runtimeRoot.."/start_screen_background_960x540_0_5_8.png")

local preview=screen:clone()
text(preview,"START",480,292,4,C.cream)
text(preview,"GUIDE",480,382,4,C.cream)
preview:saveAs(previewRoot.."/main_menu_approved_layout_preview_960x540_0_5_8.png")

local states=img(960,360)
fill(states,0,0,960,360,Color{r=11,g=8,b=6,a=255})
local sets={{startIdle,"START"},{startHover,"START"},{startPressed,"START"},{guideIdle,"GUIDE"},{guideHover,"GUIDE"},{guidePressed,"GUIDE"}}
for i,pair in ipairs(sets) do
  local col=(i<=3) and 0 or 1; local row=(i-1)%3
  local x=26+col*474; local y=14+row*112
  states:drawImage(pair[1],Point(x,y)); text(states,pair[2],x+190,y+27,3,C.cream)
end
states:saveAs(previewRoot.."/main_menu_button_states_960x360_0_5_8.png")

local composite=Sprite(960,540,ColorMode.RGB)
composite.layers[1].name="main_menu_approved_0_5_8"
composite.cels[1].image:drawImage(screen,Point(0,0))
composite:saveAs(sourceRoot.."/main_menu_approved_composite_960x540_0_5_8.aseprite")
composite:close()

local buttonSrc=Sprite(380,84,ColorMode.RGB)
buttonSrc.layers[1].name="main_menu_button_states"
buttonSrc.cels[1].image:drawImage(startIdle,Point(0,0))
for _,state in ipairs{startHover,startPressed,guideIdle,guideHover,guidePressed} do
  local f=buttonSrc:newEmptyFrame(); buttonSrc:newCel(buttonSrc.layers[1],f,state,Point(0,0))
end
buttonSrc:saveAs(sourceRoot.."/main_menu_button_states_380x84_0_5_8.aseprite")
buttonSrc:close()

