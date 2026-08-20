-- JokerHandChoice 0.5.8 for GitHub issue #70.
-- Produces an opaque modal and live-text option states without changing Joker rules.

local p=app.params
local panelPath=assert(p.panel,"panel is required")
local beforePath=assert(p.before,"before is required")
local runtimeRoot=assert(p.runtimeRoot,"runtimeRoot is required")
local sourceRoot=assert(p.sourceRoot,"sourceRoot is required")
local previewRoot=assert(p.previewRoot,"previewRoot is required")

local pc=app.pixelColor
local function img(w,h)
  local out=Image(w,h,ColorMode.RGB); out:clear(Color{r=0,g=0,b=0,a=0}); return out
end
local function fill(im,x,y,w,h,c)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function lineH(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function lineV(im,x,y,h,c,t) fill(im,x,y,t or 1,h,c) end
local function frame(im,x,y,w,h,c,t)
  t=t or 1; lineH(im,x,y,w,c,t); lineH(im,x,y+h-t,w,c,t)
  lineV(im,x,y,h,c,t); lineV(im,x+w-t,y,h,c,t)
end
local function poly(im,pts,c)
  local minY,maxY=pts[1][2],pts[1][2]
  for _,pt in ipairs(pts) do minY=math.min(minY,pt[2]); maxY=math.max(maxY,pt[2]) end
  for y=minY,maxY do
    local nodes={}; local j=#pts
    for i=1,#pts do
      local xi,yi=pts[i][1],pts[i][2]; local xj,yj=pts[j][1],pts[j][2]
      if (yi<y and yj>=y) or (yj<y and yi>=y) then nodes[#nodes+1]=math.floor(xi+(y-yi)/(yj-yi)*(xj-xi)) end
      j=i
    end
    table.sort(nodes)
    for i=1,#nodes-1,2 do lineH(im,nodes[i],y,nodes[i+1]-nodes[i]+1,c,1) end
  end
end
local function disc(im,cx,cy,r,c)
  for y=-r,r do local span=math.floor(math.sqrt(r*r-y*y)); lineH(im,cx-span,cy+y,span*2+1,c,1) end
end
local function nearest(src,w,h)
  local out=img(w,h)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w)); out:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return out
end
local function loadImage(path)
  local spr=assert(app.open(path),"cannot open "..path); local out=spr.cels[1].image:clone(); spr:close(); return out
end
local function cleanCheckerAndTrim(src)
  local cleaned=img(src.width,src.height)
  local minX,minY,maxX,maxY=src.width,src.height,-1,-1
  for y=0,src.height-1 do
    for x=0,src.width-1 do
      local v=src:getPixel(x,y); local r,g,b=pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v)
      local neutral=math.max(r,g,b)-math.min(r,g,b)<18
      local checker=neutral and r>198 and g>198 and b>198
      if not checker then
        cleaned:drawPixel(x,y,v); minX=math.min(minX,x); minY=math.min(minY,y); maxX=math.max(maxX,x); maxY=math.max(maxY,y)
      end
    end
  end
  assert(maxX>=minX and maxY>=minY,"panel has no visible pixels")
  local out=img(maxX-minX+1,maxY-minY+1)
  for y=minY,maxY do for x=minX,maxX do out:drawPixel(x-minX,y-minY,cleaned:getPixel(x,y)) end end
  return out
end

local C={
  ink=Color{r=8,g=5,b=4,a=255}, shadow=Color{r=0,g=0,b=0,a=220},
  leather=Color{r=29,g=16,b=12,a=255}, leather2=Color{r=49,g=27,b=17,a=255},
  brass0=Color{r=77,g=42,b=13,a=255}, brass1=Color{r=139,g=79,b=22,a=255},
  brass2=Color{r=211,g=144,b=43,a=255}, brass3=Color{r=248,g=203,b=104,a=255},
  cyan=Color{r=48,g=207,b=202,a=255}, cream=Color{r=241,g=225,b=184,a=255},
  muted=Color{r=111,g=96,b=78,a=255}
}
local function bolt(im,x,y)
  disc(im,x,y,3,C.ink); disc(im,x,y,2,C.brass1); fill(im,x,y-1,1,2,C.brass3)
end
local function optionButton(state)
  local out=img(340,44); local y=(state=="pressed") and 2 or 0
  fill(out,7,y+4,326,36,C.shadow)
  poly(out,{{3,y+7},{10,y+1},{330,y+1},{337,y+7},{337,y+36},{330,y+42},{10,y+42},{3,y+36}},C.ink)
  poly(out,{{7,y+8},{12,y+4},{328,y+4},{333,y+8},{333,y+35},{328,y+39},{12,y+39},{7,y+35}},C.brass0)
  fill(out,12,y+7,316,29,C.leather)
  frame(out,16,y+9,308,25,C.brass1,1)
  bolt(out,12,y+7); bolt(out,328,y+7); bolt(out,12,y+35); bolt(out,328,y+35)
  if state=="hover" then
    frame(out,12,y+6,316,31,C.brass3,2); lineH(out,38,y+10,264,C.brass2,1)
  elseif state=="selected" then
    frame(out,11,y+5,318,33,C.cyan,2); lineH(out,44,y+36,252,C.cyan,2)
  elseif state=="disabled" then
    fill(out,10,y+5,320,34,Color{r=4,g=4,b=4,a=148}); frame(out,16,y+9,308,25,C.muted,1)
  end
  return out
end

local glyphs={
  A={"01110","10001","10001","11111","10001","10001","10001"},
  B={"11110","10001","10001","11110","10001","10001","11110"},
  C={"01111","10000","10000","10000","10000","10000","01111"},
  D={"11110","10001","10001","10001","10001","10001","11110"},
  E={"11111","10000","10000","11110","10000","10000","11111"},
  F={"11111","10000","10000","11110","10000","10000","10000"},
  G={"01110","10001","10000","10111","10001","10001","01111"},
  H={"10001","10001","10001","11111","10001","10001","10001"},
  I={"11111","00100","00100","00100","00100","00100","11111"},
  J={"00111","00010","00010","00010","10010","10010","01100"},
  K={"10001","10010","10100","11000","10100","10010","10001"},
  L={"10000","10000","10000","10000","10000","10000","11111"},
  N={"10001","11001","10101","10101","10011","10001","10001"},
  O={"01110","10001","10001","10001","10001","10001","01110"},
  P={"11110","10001","10001","11110","10000","10000","10000"},
  R={"11110","10001","10001","11110","10100","10010","10001"},
  S={"01111","10000","10000","01110","00001","00001","11110"},
  T={"11111","00100","00100","00100","00100","00100","00100"},
  U={"10001","10001","10001","10001","10001","10001","01110"},
  V={"10001","10001","10001","10001","10001","01010","00100"},
  W={"10001","10001","10001","10101","10101","11011","10001"},
  Y={"10001","10001","01010","00100","00100","00100","00100"},
  [" "]={"00000","00000","00000","00000","00000","00000","00000"}
}
local function text(im,word,cx,y,scale,c)
  local total=#word*6*scale-scale; local x0=math.floor(cx-total/2)
  for i=1,#word do
    local g=glyphs[word:sub(i,i)]
    if g then
      for yy,row in ipairs(g) do for xx=1,#row do
        if row:sub(xx,xx)=="1" then fill(im,x0+(i-1)*6*scale+(xx-1)*scale,y+(yy-1)*scale,scale,scale,c) end
      end end
    end
  end
end

local panel=nearest(cleanCheckerAndTrim(loadImage(panelPath)),760,420)
local dim=img(960,540); fill(dim,0,0,960,540,Color{r=0,g=0,b=0,a=192})
local idle=optionButton("idle")
local hover=optionButton("hover")
local selected=optionButton("selected")
local disabled=optionButton("disabled")

panel:saveAs(runtimeRoot.."/joker_hand_choice_panel_760x420_0_5_8.png")
dim:saveAs(runtimeRoot.."/joker_hand_choice_dim_960x540_0_5_8.png")
idle:saveAs(runtimeRoot.."/joker_hand_option_idle_340x44_0_5_8.png")
hover:saveAs(runtimeRoot.."/joker_hand_option_hover_340x44_0_5_8.png")
selected:saveAs(runtimeRoot.."/joker_hand_option_selected_340x44_0_5_8.png")
disabled:saveAs(runtimeRoot.."/joker_hand_option_disabled_340x44_0_5_8.png")

local before=nearest(loadImage(beforePath),960,540)
local preview=before:clone(); preview:drawImage(dim,Point(0,0)); preview:drawImage(panel,Point(100,60))
text(preview,"JOKER HAND",480,198,3,C.cream)
text(preview,"CHOOSE ONE AVAILABLE HAND",480,220,1,C.muted)
local labels={"HIGH CARD","ONE PAIR","TWO PAIR","THREE OF A KIND","STRAIGHT","FLUSH","FULL HOUSE","FOUR OF A KIND","STRAIGHT FLUSH","ROYAL STRAIGHT FLUSH"}
for i,label in ipairs(labels) do
  local col=(i-1)%2; local row=math.floor((i-1)/2); local x=130+col*360; local y=236+row*46
  local state=(i==2) and hover or ((i==4) and selected or idle)
  preview:drawImage(state,Point(x,y)); text(preview,label,x+170,y+18,1,C.cream)
end
preview:saveAs(previewRoot.."/joker_hand_choice_opaque_preview_960x540_0_5_8.png")

local states=img(960,260); fill(states,0,0,960,260,Color{r=12,g=8,b=6,a=255})
local stateList={{idle,"IDLE"},{hover,"HOVER"},{selected,"SELECTED"},{disabled,"DISABLED"}}
for i,pair in ipairs(stateList) do
  local col=(i-1)%2; local row=math.floor((i-1)/2); local x=74+col*472; local y=24+row*108
  states:drawImage(pair[1],Point(x,y)); text(states,pair[2],x+170,y+18,1,C.cream)
end
states:saveAs(previewRoot.."/joker_hand_option_states_960x260_0_5_8.png")

local panelSrc=Sprite(760,420,ColorMode.RGB); panelSrc.layers[1].name="opaque_joker_hand_panel"
panelSrc.cels[1].image:drawImage(panel,Point(0,0)); panelSrc:saveAs(sourceRoot.."/joker_hand_choice_panel_760x420_0_5_8.aseprite"); panelSrc:close()
local buttonSrc=Sprite(340,44,ColorMode.RGB); buttonSrc.layers[1].name="joker_hand_option_states"
buttonSrc.cels[1].image:drawImage(idle,Point(0,0))
for _,state in ipairs{hover,selected,disabled} do local f=buttonSrc:newEmptyFrame(); buttonSrc:newCel(buttonSrc.layers[1],f,state,Point(0,0)) end
buttonSrc:saveAs(sourceRoot.."/joker_hand_option_states_340x44_0_5_8.aseprite"); buttonSrc:close()
