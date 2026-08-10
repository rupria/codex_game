-- Western shop payment animation art 0.3.8.
-- 1-2 bullets: coin-like flip from screen bottom with one-frame glint.
-- 3+ bullets: pouch tilts in from the bottom and pours rounds onto the table.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local runtimeDir=assert(p.runtimeDir)
local backgroundPath=assert(p.background)
local bulletPath=assert(p.bullet)
local bulletShinePath=assert(p.bulletShine)
local pouchPath=assert(p.pouch)

local C={
  transparent=Color{r=0,g=0,b=0,a=0},
  ink=Color{r=7,g=6,b=6,a=255},
  brassDark=Color{r=83,g=48,b=16,a=255},
  brass=Color{r=181,g=108,b=30,a=255},
  brassHi=Color{r=246,g=190,b=66,a=255},
  cream=Color{r=255,g=242,b=190,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,"cannot open "..path)
  local i=Image(s.cels[1].image); s:close(); return i
end

local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end

local function stroke(img,x,y,w,h,c,t)
  t=t or 1
  fill(img,x,y,w,t,c); fill(img,x,y+h-t,w,t,c)
  fill(img,x,y,t,h,c); fill(img,x+w-t,y,t,h,c)
end

local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function disk(img,cx,cy,r,c)
  for yy=-r,r do local span=math.floor(math.sqrt(r*r-yy*yy)); hline(img,cx-span,cx+span,cy+yy,c) end
end

local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      dst:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return dst
end

local function rotate(src,angle,size)
  local dst=Image(size,size,ColorMode.RGB); dst:clear(C.transparent)
  local ca=math.cos(angle); local sa=math.sin(angle)
  local cx=(src.width-1)/2; local cy=(src.height-1)/2; local dc=(size-1)/2
  for y=0,size-1 do for x=0,size-1 do
    local dx=x-dc; local dy=y-dc
    local sx=math.floor(ca*dx+sa*dy+cx+0.5)
    local sy=math.floor(-sa*dx+ca*dy+cy+0.5)
    if sx>=0 and sy>=0 and sx<src.width and sy<src.height then
      dst:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end end
  return dst
end

local function setImage(spr,img)
  spr.cels[1].image:clear(C.transparent)
  spr.cels[1].image:drawImage(img,Point(0,0))
end

local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB)
  setImage(s,img); s:saveAs(path); s:close()
end

local function saveRuntime(img,name)
  save(img,runtimeDir.."/"..name)
  save(img,outputDir.."/"..name)
end

local function glint(img,cx,cy,large)
  local r=large and 18 or 11
  disk(img,cx,cy,r,Color{r=181,g=108,b=30,a=22})
  hline(img,cx-r,cx+r,cy,C.brassHi)
  fill(img,cx,cy-r,1,r*2+1,C.brassHi)
  diamond(img,cx,cy,large and 6 or 4,C.cream)
  diamond(img,cx,cy,2,C.brassHi)
end

local bullet=resize(load(bulletPath),24,40)
local bulletShine=resize(load(bulletShinePath),24,40)
local bakedPouch=resize(load(pouchPath),180,150)

local function makeEmptyPouch(src)
  local dst=Image(src)
  -- Replace the five baked rounds with a dark leather interior while keeping
  -- the original alpha silhouette. Empty loops are redrawn as part of the base.
  for y=27,103 do for x=34,132 do
    local px=src:getPixel(x,y)
    if app.pixelColor.rgbaA(px)>0 then
      local shade=math.max(26,58-math.floor((y-27)*0.22))
      dst:drawPixel(x,y,app.pixelColor.rgba(shade,math.floor(shade*0.52),math.floor(shade*0.30),255))
    end
  end end
  fill(dst,39,30,91,4,Color{r=111,g=56,b=28,a=255})
  fill(dst,39,96,91,5,Color{r=31,g=16,b=11,a=255})
  local anchors={43,60,77,94,111}
  for _,x in ipairs(anchors) do
    fill(dst,x,53,13,44,Color{r=26,g=13,b=9,a=255})
    stroke(dst,x,53,13,44,Color{r=94,g=44,b=23,a=255},2)
    fill(dst,x+3,58,7,32,Color{r=54,g=26,b=16,a=255})
    fill(dst,x+3,91,7,3,Color{r=137,g=70,b=31,a=255})
  end
  return dst
end

local function capsule(img,x,y,w,h,outline,face)
  local r=math.floor(h/2)
  fill(img,x+r,y,w-r*2,h,outline)
  disk(img,x+r,y+r,r,outline); disk(img,x+w-r-1,y+r,r,outline)
  local ir=math.max(1,r-3)
  fill(img,x+r,y+3,w-r*2,h-6,face)
  disk(img,x+r,y+r,ir,face); disk(img,x+w-r-1,y+r,ir,face)
end

local function capsuleV(img,x,y,w,h,outline,face)
  local r=math.floor(w/2)
  fill(img,x,y+r,w,h-r*2,outline)
  disk(img,x+r,y+r,r,outline); disk(img,x+r,y+h-r-1,r,outline)
  local ir=math.max(1,r-3)
  fill(img,x+3,y+r,w-6,h-r*2,face)
  disk(img,x+r,y+r,ir,face); disk(img,x+r,y+h-r-1,ir,face)
end

local function makeHandCover()
  local img=Image(220,180,ColorMode.RGB); img:clear(C.transparent)
  local outline=Color{r=22,g=12,b=9,a=255}
  local glove=Color{r=74,g=37,b=23,a=255}
  local gloveHi=Color{r=126,g=67,b=37,a=255}
  -- Player-view leather glove rises from the lower edge and covers the pouch.
  capsuleV(img,48,38,25,91,outline,glove)
  capsuleV(img,76,22,26,106,outline,glove)
  capsuleV(img,105,18,27,112,outline,glove)
  capsuleV(img,135,34,25,94,outline,glove)
  disk(img,105,119,51,outline); disk(img,105,119,44,glove)
  capsule(img,132,100,68,29,outline,glove)
  fill(img,70,133,72,47,outline); fill(img,77,138,58,42,Color{r=36,g=23,b=20,a=255})
  fill(img,58,49,3,62,gloveHi); fill(img,87,34,3,72,gloveHi)
  fill(img,117,30,3,76,gloveHi); fill(img,146,47,3,60,gloveHi)
  hline(img,78,134,142,C.brassDark); hline(img,80,132,148,C.brass)
  fill(img,86,151,3,25,C.brassDark); fill(img,126,151,3,25,C.brassDark)
  return img
end

local emptyPouch=makeEmptyPouch(bakedPouch)
local handCover=makeHandCover()
saveRuntime(emptyPouch,"bar_shop_ammo_pouch_empty_180x150_0_3_8.png")
saveRuntime(bullet,"bar_shop_ammo_pouch_bullet_24x40_0_3_8.png")
saveRuntime(handCover,"bar_shop_ammo_pouch_hand_cover_220x180_0_3_8.png")

local pouch=resize(emptyPouch,112,92)

local function drawPouchCount(img,x,y,count)
  img:drawImage(emptyPouch,Point(x,y))
  if count<=5 then
    local anchors={43,60,77,94,111}
    for i=1,count do img:drawImage(bullet,Point(x+anchors[i],y+34)) end
  else
    -- Exact visual count up to the current-run theoretical maximum (30).
    -- Six columns by five rows keep every round data-driven rather than baked.
    local small=resize(bullet,10,18)
    for i=1,math.min(count,30) do
      local col=(i-1)%6; local row=math.floor((i-1)/6)
      img:drawImage(small,Point(x+44+col*13,y+31+row*13))
    end
  end
end

local countStates=Image(880,180,ColorMode.RGB); countStates:clear(C.ink)
for state=1,4 do
  local cell=Image(220,180,ColorMode.RGB); cell:clear(C.transparent)
  drawPouchCount(cell,20,15,state<=2 and 5 or 2)
  if state==2 or state==3 then cell:drawImage(handCover,Point(0,0)) end
  if state==3 then glint(cell,112,70,false) end
  countStates:drawImage(cell,Point((state-1)*220,0))
end
save(countStates,previewDir.."/bar_shop_pouch_count_change_states_880x180_0_3_8.png")
save(countStates,outputDir.."/bar_shop_pouch_count_change_states_880x180_0_3_8.png")

local countSource=Sprite(220,180,ColorMode.RGB)
countSource.layers[1].name="count_before_hand_hidden_after"
for state=1,4 do
  local cell=Image(220,180,ColorMode.RGB); cell:clear(C.transparent)
  drawPouchCount(cell,20,15,state<=2 and 5 or 2)
  if state==2 or state==3 then cell:drawImage(handCover,Point(0,0)) end
  if state==3 then glint(cell,112,70,false) end
  if state==1 then setImage(countSource,cell); countSource.frames[1].duration=0.12
  else
    local f=countSource:newEmptyFrame(); countSource:newCel(countSource.layers[1],f,cell,Point(0,0)); f.duration=0.12
  end
end
countSource:saveAs(sourceDir.."/bar_shop_pouch_count_change_states_0_3_8.aseprite")
countSource:close()

-- Flip/glint sprite sheet: 8 frames, 64x64 each.
local flipFrames={}
local flipSheet=Image(512,64,ColorMode.RGB); flipSheet:clear(C.transparent)
for i=0,7 do
  local src=(i==3 or i==4) and bulletShine or bullet
  local frame=rotate(src,i*math.pi/4,64)
  if i==3 then glint(frame,32,32,true) end
  flipFrames[i+1]=frame
  flipSheet:drawImage(frame,Point(i*64,0))
end
saveRuntime(flipSheet,"bar_shop_bullet_coin_flip_glint_8f_512x64_0_3_8.png")

local flipSource=Sprite(64,64,ColorMode.RGB)
flipSource.layers[1].name="bullet_coin_flip_glint_8f"
setImage(flipSource,flipFrames[1]); flipSource.frames[1].duration=0.07
for i=2,8 do
  local f=flipSource:newEmptyFrame()
  flipSource:newCel(flipSource.layers[1],f,flipFrames[i],Point(0,0))
  f.duration=(i==4) and 0.10 or 0.07
end
flipSource:saveAs(sourceDir.."/bar_shop_bullet_coin_flip_glint_0_3_8.aseprite")
flipSource:close()

-- Pour sprite sheet: 8 frames, 160x120 each. The origin is the lower screen edge.
local pourFrames={}
local pourSheet=Image(1280,120,ColorMode.RGB); pourSheet:clear(C.transparent)
local pouchAngles={0,-10,-24,-40,-50,-42,-24,-8}
local bulletPos={
  {},
  {{56,56,1}},
  {{48,62,2},{62,70,4}},
  {{39,72,3},{55,81,5},{72,86,6}},
  {{27,88,4},{45,91,6},{65,87,2},{83,94,7}},
  {{22,96,5},{43,86,7},{65,96,3},{87,88,1},{105,97,6}},
  {{24,98,6},{45,98,2},{66,98,7},{87,98,3},{108,98,5}},
  {{24,98,7},{45,98,3},{66,98,1},{87,98,5},{108,98,2}}
}
for i=1,8 do
  local frame=Image(160,120,ColorMode.RGB); frame:clear(C.transparent)
  if i<8 then
    local rp=rotate(pouch,pouchAngles[i]*math.pi/180,132)
    frame:drawImage(rp,Point(42,2+(i-1)*2))
  end
  for _,v in ipairs(bulletPos[i]) do
    local b=resize(flipFrames[v[3]],24,24)
    frame:drawImage(b,Point(v[1],v[2]))
  end
  if i==5 then glint(frame,66,87,false) end
  pourFrames[i]=frame
  pourSheet:drawImage(frame,Point((i-1)*160,0))
end
saveRuntime(pourSheet,"bar_shop_bullet_pour_table_8f_1280x120_0_3_8.png")

local pourSource=Sprite(160,120,ColorMode.RGB)
pourSource.layers[1].name="pouch_tilt_bullet_pour_3plus"
setImage(pourSource,pourFrames[1]); pourSource.frames[1].duration=0.09
for i=2,8 do
  local f=pourSource:newEmptyFrame()
  pourSource:newCel(pourSource.layers[1],f,pourFrames[i],Point(0,0))
  f.duration=(i>=7) and 0.14 or 0.09
end
pourSource:saveAs(sourceDir.."/bar_shop_bullet_pour_table_0_3_8.aseprite")
pourSource:close()

-- Two-row storyboard: top = 1-2 flip; bottom = 3+ pour.
local background=resize(load(backgroundPath),480,270)
local board=Image(1920,540,ColorMode.RGB); board:clear(C.ink)
local flipPts={{250,280},{258,222},{272,96},{242,126}}
for i=1,4 do
  local frame=Image(background)
  if i>1 then
    frame:drawImage(resize(flipFrames[(i-1)*2],34,34),Point(flipPts[i][1]-17,flipPts[i][2]-17))
  end
  for j=2,i-1 do diamond(frame,flipPts[j][1],flipPts[j][2],2,C.brassHi) end
  if i==3 then glint(frame,flipPts[i][1],flipPts[i][2],true) end
  board:drawImage(frame,Point((i-1)*480,0))
end

local pourPouchPos={{394,246,0},{370,222,-18},{348,206,-42},{402,255,-12}}
local pourStoryBullets={
  {},
  {{344,204,2},{326,214,4}},
  {{305,205,3},{325,221,5},{347,213,6},{365,225,2}},
  {{292,224,4},{316,224,6},{340,224,2},{364,224,7},{388,224,3}}
}
for i=1,4 do
  local frame=Image(background)
  if i<4 then
    local rp=rotate(resize(pouch,86,72),pourPouchPos[i][3]*math.pi/180,96)
    frame:drawImage(rp,Point(pourPouchPos[i][1]-48,pourPouchPos[i][2]-48))
  end
  for _,v in ipairs(pourStoryBullets[i]) do
    frame:drawImage(resize(flipFrames[v[3]],22,22),Point(v[1]-11,v[2]-11))
  end
  if i==3 then glint(frame,347,213,false) end
  board:drawImage(frame,Point((i-1)*480,270))
end
for x=478,1438,480 do fill(board,x,0,4,540,C.brassDark) end
fill(board,0,268,1920,4,C.brassDark)
save(board,previewDir.."/bar_shop_payment_modes_storyboard_1920x540_0_3_8.png")
save(board,outputDir.."/bar_shop_payment_modes_storyboard_1920x540_0_3_8.png")

print("Bar shop payment art 0.3.8 generated")
