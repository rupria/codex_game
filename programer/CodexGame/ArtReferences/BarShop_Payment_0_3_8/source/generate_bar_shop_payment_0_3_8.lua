-- Western shop payment animation art 0.3.8.
-- 1-2 bullets: coin-like flip from screen bottom with one-frame glint.
-- 3+ bullets: pouch tilts in from the bottom and pours irregularly scattered rounds onto the table.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local runtimeDir=assert(p.runtimeDir)
local backgroundPath=assert(p.background)
local bulletPath=assert(p.bullet)
local bulletShinePath=assert(p.bulletShine)
local pouchPath=assert(p.pouch)
local staticPouchPath=assert(p.staticPouch)

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
local function line(img,x0,y0,x1,y1,c)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1
  local err=dx+dy
  while true do
    img:drawPixel(x0,y0,c)
    if x0==x1 and y0==y1 then break end
    local e2=2*err
    if e2>=dy then err=err+dy; x0=x0+sx end
    if e2<=dx then err=err+dx; y0=y0+sy end
  end
end
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
local staticPouchSource=load(staticPouchPath)

local function trimAlpha(src)
  local minX=src.width; local minY=src.height; local maxX=-1; local maxY=-1
  for y=0,src.height-1 do for x=0,src.width-1 do
    if app.pixelColor.rgbaA(src:getPixel(x,y))>8 then
      if x<minX then minX=x end; if x>maxX then maxX=x end
      if y<minY then minY=y end; if y>maxY then maxY=y end
    end
  end end
  assert(maxX>=minX and maxY>=minY,"static pouch source has no opaque pixels")
  local dst=Image(maxX-minX+1,maxY-minY+1,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,dst.height-1 do for x=0,dst.width-1 do
    dst:drawPixel(x,y,src:getPixel(minX+x,minY+y))
  end end
  return dst
end

local function fitStaticPouch(src)
  local cropped=trimAlpha(src)
  local maxW=174; local maxH=144
  local scale=math.min(maxW/cropped.width,maxH/cropped.height)
  local w=math.max(1,math.floor(cropped.width*scale+0.5))
  local h=math.max(1,math.floor(cropped.height*scale+0.5))
  local dst=Image(180,150,ColorMode.RGB); dst:clear(C.transparent)
  dst:drawImage(resize(cropped,w,h),Point(math.floor((180-w)/2),math.floor((150-h)/2)))
  return dst
end

local staticPouch=fitStaticPouch(staticPouchSource)
saveRuntime(staticPouch,"bar_shop_ammo_pouch_static_5_180x150_0_3_8.png")
saveRuntime(staticPouch,"bar_shop_ammo_pouch_pile_5_180x150_0_3_8.png")
local staticPouchAse=Sprite(180,150,ColorMode.RGB)
staticPouchAse.layers[1].name="approved_static_five_round_pouch"
setImage(staticPouchAse,staticPouch)
staticPouchAse:saveAs(sourceDir.."/bar_shop_ammo_pouch_static_5_0_3_8.aseprite")
staticPouchAse:close()

local function makeEmptyPouch(src)
  local dst=Image(src)
  -- Restore the original leather body where the baked cartridge holder was.
  -- The fill intentionally blends into the pouch instead of leaving a square UI frame.
  for y=27,103 do for x=34,132 do
    local px=src:getPixel(x,y)
    if app.pixelColor.rgbaA(px)>0 then
      local shade=math.max(46,88-math.floor((y-27)*0.30))
      dst:drawPixel(x,y,app.pixelColor.rgba(shade,math.floor(shade*0.48),math.floor(shade*0.28),255))
    end
  end end
  -- Rounded open pocket. No rectangular border, holder slots, or panel corners.
  local cavity=Color{r=28,g=14,b=10,a=255}
  local cavityDeep=Color{r=17,g=9,b=7,a=255}
  fill(dst,54,50,55,39,cavity)
  disk(dst,54,69,19,cavity); disk(dst,108,69,19,cavity)
  fill(dst,58,56,47,29,cavityDeep)
  disk(dst,58,70,14,cavityDeep); disk(dst,104,70,14,cavityDeep)
  -- Curved leather lip and a soft inner seam reinforce an actual bag opening.
  line(dst,47,56,56,49,Color{r=132,g=67,b=32,a=255})
  line(dst,56,49,107,49,Color{r=132,g=67,b=32,a=255})
  line(dst,107,49,119,57,Color{r=132,g=67,b=32,a=255})
  line(dst,48,58,57,53,Color{r=72,g=34,b=20,a=255})
  line(dst,57,53,106,53,Color{r=72,g=34,b=20,a=255})
  line(dst,106,53,117,59,Color{r=72,g=34,b=20,a=255})
  line(dst,51,87,61,92,Color{r=91,g=43,b=22,a=255})
  line(dst,61,92,108,92,Color{r=91,g=43,b=22,a=255})
  line(dst,108,92,116,87,Color{r=91,g=43,b=22,a=255})
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
  -- Loose pile: rounds overlap at different angles and heights instead of
  -- standing in holders. Every round lies horizontally inside the open pocket.
  if count<=5 then
    local layouts={
      [1]={{69,52,1.45}},
      [2]={{54,53,1.28},{80,54,-1.32}},
      [3]={{45,53,1.18},{68,60,-1.43},{89,50,1.48}},
      [4]={{43,51,1.15},{65,48,-1.34},{84,58,1.42},{58,64,-1.48}},
      [5]={{42,49,1.17},{62,46,-1.36},{84,50,1.46},{52,63,-1.50},{76,64,1.24}}
    }
    for _,v in ipairs(layouts[count] or {}) do
      local rb=rotate(bullet,v[3],44)
      img:drawImage(rb,Point(x+v[1],y+v[2]))
    end
  else
    -- Exact visual count up to 30 in crossed horizontal layers, never a grid.
    local small=resize(bullet,10,18)
    local shifts={0,5,-3,4,-5,2}
    local angles={1.18,-1.24,1.42,-1.36,1.06,-1.48}
    for i=1,math.min(count,30) do
      local col=(i-1)%6; local row=math.floor((i-1)/6)
      local rb=rotate(small,angles[(i%6)+1],22)
      img:drawImage(rb,Point(x+39+col*13+shifts[(row%6)+1],y+44+row*9+(col%2)*3))
    end
  end
end

-- The legacy dynamic count preview remains reproducible for reference only.
-- Runtime must bind the approved static image above and must not add bullet children.

local countStates=Image(880,180,ColorMode.RGB); countStates:clear(C.ink)
for state=1,4 do
  local cell=Image(220,180,ColorMode.RGB); cell:clear(C.transparent)
  drawPouchCount(cell,20,15,6-state)
  if state==2 then glint(cell,132,74,false) end
  countStates:drawImage(cell,Point((state-1)*220,0))
end
save(countStates,previewDir.."/bar_shop_pouch_count_change_states_880x180_0_3_8.png")
save(countStates,outputDir.."/bar_shop_pouch_count_change_states_880x180_0_3_8.png")

local countSource=Sprite(220,180,ColorMode.RGB)
countSource.layers[1].name="loose_pile_count_decrease_no_hand"
for state=1,4 do
  local cell=Image(220,180,ColorMode.RGB); cell:clear(C.transparent)
  drawPouchCount(cell,20,15,6-state)
  if state==2 then glint(cell,132,74,false) end
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
-- Landed rounds deliberately vary in x/y spacing and rotation; never snap them to a row or grid.
local pourFrames={}
local pourSheet=Image(1280,120,ColorMode.RGB); pourSheet:clear(C.transparent)
local pouchAngles={0,-10,-24,-40,-50,-42,-24,-8}
local bulletPos={
  {},
  {{56,56,1}},
  {{48,62,2},{62,70,4}},
  {{39,72,3},{55,81,5},{72,86,6}},
  {{25,89,4},{48,95,6},{67,81,2},{89,96,7}},
  {{18,98,5},{43,83,7},{70,101,3},{96,87,1},{116,97,6}},
  {{16,101,6},{45,87,2},{72,103,7},{99,92,3},{124,100,5}},
  {{17,101,7},{49,88,3},{68,103,1},{101,92,5},{126,101,2}}
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
  {{302,197,3},{327,216,5},{350,202,6},{369,220,2}},
  {{286,187,4},{318,169,6},{346,194,2},{378,176,7},{406,199,3}}
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
