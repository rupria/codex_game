-- Joker reveal presentation art 0.5.3.
-- Reuses approved Joker fronts; produces only frames, VFX and visual QA boards.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local backgroundPath=assert(p.background)
local trayPath=assert(p.tray)
local jokerBrassPath=assert(p.jokerBrass)
local jokerCrimsonPath=assert(p.jokerCrimson)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, ink=Color{r=8,g=7,b=6,a=255},
  shade=Color{r=3,g=4,b=5,a=210}, panel=Color{r=19,g=15,b=12,a=246},
  leather=Color{r=54,g=28,b=15,a=255}, brassDark=Color{r=91,g=51,b=16,a=255},
  brass=Color{r=185,g=113,b=30,a=255}, brassHi=Color{r=249,g=197,b=80,a=255},
  cream=Color{r=255,g=234,b=178,a=255}, red=Color{r=174,g=43,b=39,a=255},
  redHi=Color{r=255,g=82,b=49,a=255}, teal=Color{r=37,g=190,b=191,a=255},
  smoke=Color{r=45,g=37,b=31,a=190}, white=Color{r=255,g=248,b=218,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,'cannot open '..path)
  local i=Image(s.cels[1].image); s:close(); return i
end
local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end
local function line(img,x0,y0,x1,y1,c)
  x0=math.floor(x0); y0=math.floor(y0); x1=math.floor(x1); y1=math.floor(y1)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1; local err=dx+dy
  while true do
    if x0>=0 and y0>=0 and x0<img.width and y0<img.height then img:drawPixel(x0,y0,c) end
    if x0==x1 and y0==y1 then break end
    local e2=2*err
    if e2>=dy then err=err+dy; x0=x0+sx end
    if e2<=dx then err=err+dx; y0=y0+sy end
  end
end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function star(img,cx,cy,r,c)
  diamond(img,cx,cy,r,c)
  hline(img,cx-r*2,cx+r*2,cy,c); vline(img,cx,cy-r*2,cy+r*2,c)
  line(img,cx-r,cy-r,cx+r,cy+r,c); line(img,cx-r,cy+r,cx+r,cy-r,c)
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.clear)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); s.cels[1].image:clear(C.clear)
  s.cels[1].image:drawImage(img,Point(0,0)); s:saveAs(path); s:close()
end
local function saveRuntime(img,name) save(img,runtimeDir..'/'..name) end
local function border(img,x,y,w,h,active)
  local edge=active and C.brassHi or C.brass
  fill(img,x,y,w,h,C.ink); fill(img,x+2,y+2,w-4,h-4,C.brassDark)
  fill(img,x+4,y+4,w-8,h-8,C.clear)
  hline(img,x+12,x+w-13,y+5,edge); hline(img,x+12,x+w-13,y+h-6,edge)
  vline(img,x+5,y+12,y+h-13,edge); vline(img,x+w-6,y+12,y+h-13,edge)
  diamond(img,x+6,y+6,4,C.brassHi); diamond(img,x+w-7,y+6,4,C.brassHi)
  diamond(img,x+6,y+h-7,4,C.brass); diamond(img,x+w-7,y+h-7,4,C.brass)
end

local background=load(backgroundPath)
if background.width~=960 or background.height~=540 then background=resize(background,960,540) end
local tray=load(trayPath)
local jokerBrass=load(jokerBrassPath)
local jokerCrimson=load(jokerCrimsonPath)
local brassCard=resize(jokerBrass,112,156)
local crimsonCard=resize(jokerCrimson,112,156)

local vignette=Image(960,540,ColorMode.RGB); vignette:clear(C.clear)
for i=0,11 do
  local a=math.floor(10+i*8)
  local cc=Color{r=2,g=3,b=4,a=a}
  fill(vignette,0,i*10,960,10,cc); fill(vignette,0,530-i*10,960,10,cc)
  fill(vignette,i*14,0,14,540,cc); fill(vignette,946-i*14,0,14,540,cc)
end
fill(vignette,0,0,960,54,Color{r=2,g=3,b=4,a=100})
fill(vignette,0,472,960,68,Color{r=2,g=3,b=4,a=120})
saveRuntime(vignette,'joker_reveal_focus_vignette_960x540_0_5_3.png')

local frameIdle=Image(140,190,ColorMode.RGB); frameIdle:clear(C.clear); border(frameIdle,0,0,140,190,false)
local frameActive=Image(140,190,ColorMode.RGB); frameActive:clear(C.clear); border(frameActive,0,0,140,190,true)
star(frameActive,70,9,3,C.cream); diamond(frameActive,12,95,3,C.redHi); diamond(frameActive,127,95,3,C.redHi)
saveRuntime(frameIdle,'joker_reveal_card_frame_idle_140x190_0_5_3.png')
saveRuntime(frameActive,'joker_reveal_card_frame_active_140x190_0_5_3.png')

local badge=Image(64,64,ColorMode.RGB); badge:clear(C.clear)
star(badge,32,32,13,C.ink); star(badge,32,32,11,C.brass); diamond(badge,32,32,7,C.leather)
diamond(badge,32,32,4,C.redHi); hline(badge,29,35,32,C.cream)
saveRuntime(badge,'joker_reveal_badge_64_0_5_3.png')

local aiMarker=Image(48,48,ColorMode.RGB); aiMarker:clear(C.clear)
star(aiMarker,24,24,9,C.ink); star(aiMarker,24,24,7,C.brass)
fill(aiMarker,19,14,10,20,C.panel); hline(aiMarker,20,28,16,C.cream); diamond(aiMarker,24,26,3,C.redHi)
saveRuntime(aiMarker,'joker_ai_showdown_marker_48_0_5_3.png')

local glintFrames={}; local glintSheet=Image(384,64,ColorMode.RGB); glintSheet:clear(C.clear)
for i=1,6 do
  local fr=Image(64,64,ColorMode.RGB); fr:clear(C.clear)
  local radius=math.max(1,math.min(i,7-i)*3)
  star(fr,32,32,radius,C.brassHi)
  if i==3 or i==4 then star(fr,32,32,5,C.white) end
  glintFrames[i]=fr; glintSheet:drawImage(fr,Point((i-1)*64,0))
end
saveRuntime(glintSheet,'joker_reveal_glint_6f_384x64_0_5_3.png')

local burstFrames={}; local burstSheet=Image(512,64,ColorMode.RGB); burstSheet:clear(C.clear)
for i=1,8 do
  local fr=Image(64,64,ColorMode.RGB); fr:clear(C.clear)
  if i<=5 then
    local r=2+i*3; star(fr,32,32,r,(i<=2) and C.white or C.brassHi)
    diamond(fr,32,32,math.max(1,8-i),C.redHi)
  end
  local count=math.max(0,i-2)*2
  for n=1,count do
    local ang=(n*1.7+i*0.4); local rr=8+i*3+((n*7)%6)
    local x=math.floor(32+math.cos(ang)*rr); local y=math.floor(32+math.sin(ang)*rr)
    diamond(fr,x,y,(n%3==0) and 2 or 1,(n%2==0) and C.brassHi or C.redHi)
  end
  if i>=6 then fill(fr,18+i,21,18,18,Color{r=45,g=37,b=31,a=math.max(30,200-i*20)}) end
  burstFrames[i]=fr; burstSheet:drawImage(fr,Point((i-1)*64,0))
end
saveRuntime(burstSheet,'joker_reveal_burst_8f_512x64_0_5_3.png')

local glowFrames={}; local glowSheet=Image(840,190,ColorMode.RGB); glowSheet:clear(C.clear)
for i=1,6 do
  local fr=Image(140,190,ColorMode.RGB); fr:clear(C.clear); border(fr,0,0,140,190,true)
  local offset=(i-1)%3
  star(fr,70,9,2+offset,C.cream)
  diamond(fr,9,95,2+((i+1)%2),C.redHi); diamond(fr,130,95,2+((i+1)%2),C.redHi)
  glowFrames[i]=fr; glowSheet:drawImage(fr,Point((i-1)*140,0))
end
saveRuntime(glowSheet,'joker_reveal_frame_glow_6f_840x190_0_5_3.png')

local function drawCardWithFrame(img,card,cx,cy,scale,active)
  local fw=math.floor(140*scale); local fh=math.floor(190*scale)
  local cardW=math.floor(112*scale); local cardH=math.floor(156*scale)
  img:drawImage(resize(active and frameActive or frameIdle,fw,fh),Point(math.floor(cx-fw/2),math.floor(cy-fh/2)))
  img:drawImage(resize(card,cardW,cardH),Point(math.floor(cx-cardW/2),math.floor(cy-cardH/2)))
end
local function baseScene()
  local img=Image(background); img:drawImage(vignette,Point(0,0)); return img
end
local function titleSafe(img)
  fill(img,300,44,360,52,C.ink); fill(img,303,47,354,46,C.panel)
  hline(img,324,636,50,C.brass); hline(img,324,636,89,C.brassDark)
  diamond(img,311,70,5,C.brassHi); diamond(img,648,70,5,C.brassHi)
end
local function candidateRail(img)
  fill(img,275,404,410,96,Color{r=8,g=10,b=11,a=238})
  hline(img,293,667,405,C.brass); hline(img,293,667,498,C.brassDark)
  for i=0,2 do border(img,320+i*116,414,104,74,false) end
end

local scenes={}
-- 1: focus after Halli result.
scenes[1]=baseScene(); scenes[1]:drawImage(tray,Point(22,390)); titleSafe(scenes[1]); star(scenes[1],480,270,8,C.brass)
-- 2: the approved card rises from the player tray.
scenes[2]=baseScene(); scenes[2]:drawImage(tray,Point(22,390)); titleSafe(scenes[2]); drawCardWithFrame(scenes[2],brassCard,250,344,0.62,false)
line(scenes[2],118,430,220,365,C.brassHi); diamond(scenes[2],215,368,4,C.brassHi)
-- 3: flip edge, no face leak until midpoint.
scenes[3]=baseScene(); scenes[3]:drawImage(tray,Point(22,390)); titleSafe(scenes[3]); fill(scenes[3],474,170,12,190,C.ink); fill(scenes[3],477,174,6,182,C.brassHi)
star(scenes[3],480,270,12,C.brass)
-- 4: player full reveal.
scenes[4]=baseScene(); titleSafe(scenes[4]); drawCardWithFrame(scenes[4],brassCard,480,270,1.0,true); star(scenes[4],365,260,10,C.redHi); star(scenes[4],595,286,8,C.brassHi)
-- 5: settles into the actual candidate rail.
scenes[5]=baseScene(); titleSafe(scenes[5]); candidateRail(scenes[5]); drawCardWithFrame(scenes[5],brassCard,596,452,0.42,true); drawCardWithFrame(scenes[5],crimsonCard,364,452,0.42,false)
line(scenes[5],480,334,568,414,C.brassHi); diamond(scenes[5],565,411,4,C.brassHi)
-- 6: AI reduced marker, only legal at showdown.
scenes[6]=baseScene(); titleSafe(scenes[6]); candidateRail(scenes[6]); scenes[6]:drawImage(aiMarker,Point(456,226)); drawCardWithFrame(scenes[6],crimsonCard,480,316,0.58,true)

local board=Image(2880,1080,ColorMode.RGB); board:clear(C.ink)
for i=1,6 do local col=(i-1)%3; local row=math.floor((i-1)/3); board:drawImage(scenes[i],Point(col*960,row*540)) end
save(board,previewDir..'/joker_reveal_storyboard_6cut_2880x1080_0_5_3.png')
save(scenes[4],previewDir..'/joker_reveal_player_focus_960x540_0_5_3.png')
save(scenes[6],previewDir..'/joker_reveal_ai_showdown_compact_960x540_0_5_3.png')

local function safePreview(w,h,path)
  local canvas=resize(scenes[4],w,h)
  local s=w/960; local t=h/540
  local x=math.floor(24*s); local y=math.floor(24*t); local ww=w-2*x; local hh=h-2*y
  hline(canvas,x,x+ww,y,C.teal); hline(canvas,x,x+ww,y+hh,C.teal)
  vline(canvas,x,y,y+hh,C.teal); vline(canvas,x+ww,y,y+hh,C.teal)
  save(canvas,path)
end
safePreview(960,540,previewDir..'/joker_reveal_safe_960x540_0_5_3.png')
safePreview(1280,720,previewDir..'/joker_reveal_safe_1280x720_0_5_3.png')
safePreview(1920,1080,previewDir..'/joker_reveal_safe_1920x1080_0_5_3.png')

local source=Sprite(140,190,ColorMode.RGB); source.layers[1].name='idle_active_glow_6f'
source.cels[1].image:clear(C.clear); source.cels[1].image:drawImage(frameIdle,Point(0,0))
local f2=source:newEmptyFrame(); source:newCel(source.layers[1],f2,frameActive,Point(0,0))
for i=1,6 do local f=source:newEmptyFrame(); source:newCel(source.layers[1],f,glowFrames[i],Point(0,0)); f.duration=0.08 end
source:saveAs(sourceDir..'/joker_reveal_frame_states_0_5_3.aseprite'); source:close()

local fx=Sprite(64,64,ColorMode.RGB); fx.layers[1].name='burst_8f'
fx.cels[1].image:clear(C.clear); fx.cels[1].image:drawImage(burstFrames[1],Point(0,0)); fx.frames[1].duration=0.05
for i=2,8 do local f=fx:newEmptyFrame(); fx:newCel(fx.layers[1],f,burstFrames[i],Point(0,0)); f.duration=(i>=6) and 0.09 or 0.05 end
fx:saveAs(sourceDir..'/joker_reveal_burst_0_5_3.aseprite'); fx:close()

local review=Sprite(960,540,ColorMode.RGB); review.layers[1].name='approved_player_reveal_focus'; review.cels[1].image:drawImage(scenes[4],Point(0,0)); review:saveAs(sourceDir..'/joker_reveal_player_focus_0_5_3.aseprite'); review:close()

print('Joker reveal art 0.5.3 generated')
