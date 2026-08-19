-- Joker reveal presentation art 0.5.4 review package.
-- Local review output only. Reuses the approved Joker card and current saloon board.

local p=app.params
local outDir=assert(p.outDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local backgroundPath=assert(p.background)
local cardPath=assert(p.card)
local cardBackPath=assert(p.cardBack)

local C={
  clear=Color{r=0,g=0,b=0,a=0}, ink=Color{r=7,g=6,b=5,a=255},
  shade=Color{r=1,g=2,b=3,a=170}, smoke=Color{r=52,g=42,b=33,a=180},
  brassDark=Color{r=82,g=47,b=17,a=255}, brass=Color{r=184,g=111,b=30,a=255},
  brassHi=Color{r=249,g=194,b=72,a=255}, cream=Color{r=255,g=238,b=188,a=255},
  red=Color{r=185,g=48,b=38,a=255}, redHi=Color{r=255,g=93,b=45,a=255},
  teal=Color{r=35,g=177,b=184,a=255}, tealDark=Color{r=13,g=91,b=103,a=220},
  white=Color{r=255,g=252,b=224,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,'cannot open '..path)
  local i=Image(s.cels[1].image); s:close(); return i
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB)
  s.cels[1].image:clear(C.clear); s.cels[1].image:drawImage(img,Point(0,0))
  s:saveAs(path); s:close()
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.clear)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      dst:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return dst
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
  diamond(img,cx,cy,math.max(1,math.floor(r/2)),c)
  hline(img,cx-r,cx+r,cy,c); vline(img,cx,cy-r,cy+r,c)
  line(img,cx-math.floor(r/2),cy-math.floor(r/2),cx+math.floor(r/2),cy+math.floor(r/2),c)
  line(img,cx-math.floor(r/2),cy+math.floor(r/2),cx+math.floor(r/2),cy-math.floor(r/2),c)
end
local function ring(img,cx,cy,r,thickness,c,phase,segments)
  phase=phase or 0; segments=segments or 1
  for deg=0,359 do
    local seg=math.floor(((deg+phase)%360)/(360/segments))
    if segments==1 or seg%2==0 then
      local rad=math.rad(deg)
      for t=0,thickness-1 do
        local rr=r-t
        local x=math.floor(cx+math.cos(rad)*rr+0.5)
        local y=math.floor(cy+math.sin(rad)*rr+0.5)
        if x>=0 and y>=0 and x<img.width and y<img.height then img:drawPixel(x,y,c) end
      end
    end
  end
end
local function sheet(frames,w,h)
  local img=Image(w*#frames,h,ColorMode.RGB); img:clear(C.clear)
  for i=1,#frames do img:drawImage(frames[i],Point((i-1)*w,0)) end
  return img
end
local function saveAnimated(frames,durations,path,name)
  local s=Sprite(frames[1].width,frames[1].height,ColorMode.RGB)
  s.layers[1].name=name
  s.cels[1].image:clear(C.clear); s.cels[1].image:drawImage(frames[1],Point(0,0))
  s.frames[1].duration=durations[1]
  for i=2,#frames do
    local f=s:newEmptyFrame(); f.duration=durations[i]
    s:newCel(s.layers[1],f,frames[i],Point(0,0))
  end
  s:saveAs(path); s:close()
end

local background=load(backgroundPath)
if background.width~=960 or background.height~=540 then background=resize(background,960,540) end
local card=resize(load(cardPath),112,156)
local cardBack=resize(load(cardBackPath),80,112)

-- Darkens the playfield without obscuring the approved table or UI-safe margins.
local vignette=Image(960,540,ColorMode.RGB); vignette:clear(C.clear)
for i=0,11 do
  local alpha=math.floor(12+i*9); local c=Color{r=1,g=2,b=3,a=alpha}
  fill(vignette,0,i*9,960,9,c); fill(vignette,0,531-i*9,960,9,c)
  fill(vignette,i*13,0,13,540,c); fill(vignette,947-i*13,0,13,540,c)
end
fill(vignette,0,0,960,72,Color{r=1,g=2,b=3,a=120})
save(vignette,outDir..'/joker_reveal_focus_vignette_960x540_0_5_4.png')

-- 6-frame teal-to-brass rising arc trail.
local trailFrames={}
for i=1,6 do
  local fr=Image(96,96,ColorMode.RGB); fr:clear(C.clear)
  local visible=math.max(1,i)
  for n=1,visible*4 do
    local t=n/(visible*4)
    local x=math.floor(18+58*t)
    local y=math.floor(78-56*t-13*math.sin(t*math.pi))
    diamond(fr,x,y,(n%7==0) and 2 or 1,(t>0.68) and C.brassHi or C.teal)
  end
  if i>=3 then star(fr,75,18,2+(i%2),C.brassHi) end
  trailFrames[i]=fr
end
save(sheet(trailFrames,96,96),outDir..'/joker_reveal_arc_trail_6f_576x96_0_5_4.png')
saveAnimated(trailFrames,{0.05,0.05,0.05,0.05,0.06,0.08},sourceDir..'/joker_reveal_arc_trail_0_5_4.aseprite','arc_trail_6f')

-- 8-frame restrained revolver-cylinder / gunsight ring.
local sightFrames={}
for i=1,8 do
  local fr=Image(192,192,ColorMode.RGB); fr:clear(C.clear)
  local r=math.min(88,24+i*9)
  local fade=i>=7
  local edge=fade and C.brass or C.brassHi
  ring(fr,96,96,r,2,edge,i*7,8)
  if i>=4 then ring(fr,96,96,r-5,1,C.brassDark,i*7,8) end
  if i>=3 then
    for n=0,5 do
      local a=math.rad(n*60+i*3); local x=math.floor(96+math.cos(a)*58); local y=math.floor(96+math.sin(a)*58)
      ring(fr,x,y,9,1,C.brassDark,0,1)
      diamond(fr,x,y,2,C.brass)
    end
  end
  if i>=4 and i<=7 then
    hline(fr,3,28,96,C.brassHi); hline(fr,163,188,96,C.brassHi)
    vline(fr,96,3,28,C.brassHi); vline(fr,96,163,188,C.brassHi)
  end
  sightFrames[i]=fr
end
save(sheet(sightFrames,192,192),outDir..'/joker_reveal_gunsight_ring_8f_1536x192_0_5_4.png')
saveAnimated(sightFrames,{0.04,0.04,0.04,0.05,0.06,0.07,0.08,0.10},sourceDir..'/joker_reveal_gunsight_ring_0_5_4.aseprite','gunsight_ring_8f')

-- 8-frame compact flash: bright for two frames, then embers and smoke.
local flashFrames={}
for i=1,8 do
  local fr=Image(160,160,ColorMode.RGB); fr:clear(C.clear)
  if i<=4 then
    local r=12+i*14
    star(fr,80,80,r,(i<=2) and C.white or C.brassHi)
    diamond(fr,80,80,math.max(3,14-i*2),C.redHi)
    for n=1,6+i do
      local a=math.rad((n*47+i*13)%360); local rr=18+i*4+(n%4)*3
      diamond(fr,math.floor(80+math.cos(a)*rr),math.floor(80+math.sin(a)*rr),1,(n%2==0) and C.brassHi or C.red)
    end
  else
    local alpha=math.max(35,180-(i-4)*35)
    local smoke=Color{r=52,g=42,b=33,a=alpha}
    diamond(fr,68+(i-4)*2,79-(i-4)*3,10,smoke)
    diamond(fr,91+(i-4),73-(i-4)*4,9,smoke)
    for n=1,10-i do diamond(fr,50+n*8,104-(n%3)*5,1,C.brass) end
  end
  flashFrames[i]=fr
end
save(sheet(flashFrames,160,160),outDir..'/joker_reveal_muzzle_flash_8f_1280x160_0_5_4.png')
saveAnimated(flashFrames,{0.035,0.035,0.045,0.055,0.07,0.08,0.09,0.10},sourceDir..'/joker_reveal_muzzle_flash_0_5_4.aseprite','muzzle_flash_8f')

-- 6-frame diagonal highlight sized exactly for the 112x156 Joker front.
local glintFrames={}
for i=1,6 do
  local fr=Image(112,156,ColorMode.RGB); fr:clear(C.clear)
  local x=-28+(i-1)*34
  for d=-2,2 do line(fr,x+d,154,x+55+d,0,(d==0) and C.white or C.brassHi) end
  star(fr,x+28,76,2+(i%2),C.white)
  glintFrames[i]=fr
end
save(sheet(glintFrames,112,156),outDir..'/joker_reveal_card_glint_6f_672x156_0_5_4.png')
saveAnimated(glintFrames,{0.06,0.06,0.06,0.06,0.06,0.08},sourceDir..'/joker_reveal_card_glint_0_5_4.aseprite','card_glint_6f')

local settleFrames={}
for i=1,5 do
  local fr=Image(64,64,ColorMode.RGB); fr:clear(C.clear)
  local r=(i<=3) and (i*3) or ((6-i)*3)
  if r>0 then star(fr,32,32,r,(i==3) and C.white or C.brassHi) end
  settleFrames[i]=fr
end
save(sheet(settleFrames,64,64),outDir..'/joker_reveal_settle_glint_5f_320x64_0_5_4.png')
saveAnimated(settleFrames,{0.05,0.05,0.06,0.07,0.09},sourceDir..'/joker_reveal_settle_glint_0_5_4.aseprite','settle_glint_5f')

local function sceneBase()
  local img=Image(background); img:drawImage(vignette,Point(0,0)); return img
end
local function drawCard(img,cx,cy,w,h)
  img:drawImage(resize(card,w,h),Point(math.floor(cx-w/2),math.floor(cy-h/2)))
end
local scenes={}
scenes[1]=sceneBase(); scenes[1]:drawImage(cardBack,Point(440,400)); ring(scenes[1],480,456,48,1,C.brassDark,0,8)
scenes[2]=sceneBase(); scenes[2]:drawImage(resize(trailFrames[5],144,144),Point(398,256)); drawCard(scenes[2],480,302,92,128)
scenes[3]=sceneBase(); scenes[3]:drawImage(sightFrames[6],Point(384,174)); scenes[3]:drawImage(flashFrames[3],Point(400,190)); drawCard(scenes[3],480,270,112,156); scenes[3]:drawImage(glintFrames[4],Point(424,192))
scenes[4]=sceneBase(); drawCard(scenes[4],590,418,84,117); scenes[4]:drawImage(settleFrames[3],Point(600,340)); ring(scenes[4],590,418,54,1,C.brass,0,8)

local board=Image(1920,1080,ColorMode.RGB); board:clear(C.ink)
for i=1,4 do local col=(i-1)%2; local row=math.floor((i-1)/2); board:drawImage(scenes[i],Point(col*960,row*540)) end
save(board,previewDir..'/joker_reveal_storyboard_4cut_1920x1080_0_5_4.png')
save(scenes[3],previewDir..'/joker_reveal_focus_960x540_0_5_4.png')
save(scenes[4],previewDir..'/joker_reveal_settle_960x540_0_5_4.png')

print('Joker reveal review art 0.5.4 generated')
