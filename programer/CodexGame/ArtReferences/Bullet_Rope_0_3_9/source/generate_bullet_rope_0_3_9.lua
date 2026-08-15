-- Western bullet and Halli rope-timer art 0.3.9.
-- Bullet: readable copper projectile, brass case, rim and directional metal glint.
-- Rope: thick three-strand western fuse, animated ember tip and gunpowder burst.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local barRuntimeDir=assert(p.barRuntimeDir)
local halliRuntimeDir=assert(p.halliRuntimeDir)
local oldBulletPath=assert(p.oldBullet)
local pouchPath=assert(p.pouch)
local halliBackgroundPath=assert(p.halliBackground)

local C={
  transparent=Color{r=0,g=0,b=0,a=0},
  ink=Color{r=10,g=7,b=5,a=255},
  copperDeep=Color{r=75,g=30,b=18,a=255},
  copper=Color{r=154,g=67,b=31,a=255},
  copperHi=Color{r=232,g=127,b=55,a=255},
  brassDeep=Color{r=73,g=42,b=13,a=255},
  brassDark=Color{r=119,g=68,b=18,a=255},
  brass=Color{r=190,g=118,b=31,a=255},
  brassHi=Color{r=249,g=194,b=73,a=255},
  brassWhite=Color{r=255,g=235,b=156,a=255},
  steel=Color{r=75,g=73,b=67,a=255},
  ropeShadow=Color{r=37,g=22,b=13,a=255},
  ropeDark=Color{r=74,g=43,b=22,a=255},
  rope=Color{r=137,g=91,b=44,a=255},
  ropeHi=Color{r=211,g=159,b=87,a=255},
  ember=Color{r=255,g=80,b=10,a=255},
  orange=Color{r=255,g=132,b=15,a=255},
  yellow=Color{r=255,g=216,b=55,a=255},
  cream=Color{r=255,g=244,b=180,a=255},
  smoke=Color{r=79,g=70,b=65,a=190},
  smokeDark=Color{r=41,g=37,b=35,a=210},
  panel=Color{r=14,g=12,b=11,a=255}
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

local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end

local function line(img,x0,y0,x1,y1,c)
  x0=math.floor(x0); y0=math.floor(y0); x1=math.floor(x1); y1=math.floor(y1)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1
  local err=dx+dy
  while true do
    if x0>=0 and y0>=0 and x0<img.width and y0<img.height then img:drawPixel(x0,y0,c) end
    if x0==x1 and y0==y1 then break end
    local e2=2*err
    if e2>=dy then err=err+dy; x0=x0+sx end
    if e2<=dx then err=err+dx; y0=y0+sy end
  end
end

local function disk(img,cx,cy,r,c)
  for yy=-r,r do
    local span=math.floor(math.sqrt(math.max(0,r*r-yy*yy)))
    hline(img,cx-span,cx+span,cy+yy,c)
  end
end

local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local span=r-math.abs(yy); hline(img,cx-span,cx+span,cy+yy,c) end
end

local function triangle(img,x1,y1,x2,y2,x3,y3,c)
  local minx=math.floor(math.min(x1,x2,x3)); local maxx=math.ceil(math.max(x1,x2,x3))
  local miny=math.floor(math.min(y1,y2,y3)); local maxy=math.ceil(math.max(y1,y2,y3))
  local function sign(px,py,ax,ay,bx,by) return (px-bx)*(ay-by)-(ax-bx)*(py-by) end
  for y=miny,maxy do for x=minx,maxx do
    local d1=sign(x,y,x1,y1,x2,y2); local d2=sign(x,y,x2,y2,x3,y3); local d3=sign(x,y,x3,y3,x1,y1)
    local neg=(d1<0) or (d2<0) or (d3<0); local pos=(d1>0) or (d2>0) or (d3>0)
    if not (neg and pos) and x>=0 and y>=0 and x<img.width and y<img.height then img:drawPixel(x,y,c) end
  end end
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

local function crop(src,w,h)
  local dst=Image(math.max(1,w),h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do for x=0,math.min(w,src.width)-1 do dst:drawPixel(x,y,src:getPixel(x,y)) end end
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
    if sx>=0 and sy>=0 and sx<src.width and sy<src.height then dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end end
  return dst
end

local function setImage(spr,img)
  spr.cels[1].image:clear(C.transparent)
  spr.cels[1].image:drawImage(img,Point(0,0))
end

local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close()
end

local function saveBar(img,name)
  save(img,barRuntimeDir.."/"..name); save(img,outputDir.."/"..name)
end

local function saveHalli(img,name)
  save(img,halliRuntimeDir.."/"..name); save(img,outputDir.."/"..name)
end

local function bulletImage(state)
  local img=Image(24,40,ColorMode.RGB); img:clear(C.transparent)
  -- Strong one-pixel silhouette with a tapered projectile and readable case rim.
  fill(img,10,0,4,1,C.ink); fill(img,8,1,8,1,C.ink); fill(img,7,2,10,2,C.ink)
  fill(img,6,4,12,8,C.ink); fill(img,7,12,10,3,C.ink)
  fill(img,5,15,14,18,C.ink); fill(img,3,33,18,5,C.ink); fill(img,6,38,12,2,C.ink)
  -- Copper jacket: rounded nose, side reflection and dark cannelure.
  fill(img,10,1,4,1,C.copperHi); fill(img,8,2,8,2,C.copper)
  fill(img,7,4,10,6,C.copper); fill(img,8,4,3,5,C.copperHi)
  fill(img,15,5,2,5,C.copperDeep); fill(img,7,10,10,2,C.copperDeep)
  hline(img,8,15,12,C.steel); fill(img,8,13,8,1,C.copperDark)
  -- Neck and brass case: four tones preserve volume at UI scale.
  fill(img,7,14,10,3,C.brassDark); fill(img,6,17,12,15,C.brass)
  fill(img,7,18,3,12,C.brassHi); fill(img,10,18,2,12,C.brassWhite)
  fill(img,15,18,3,13,C.brassDeep); hline(img,6,17,22,C.brassHi)
  hline(img,6,17,30,C.brassDark); fill(img,5,31,14,2,C.brassDeep)
  -- Extractor rim and primer cue.
  fill(img,4,33,16,4,C.brass); hline(img,4,19,33,C.brassWhite)
  fill(img,6,37,12,1,C.brassDeep); fill(img,8,38,8,1,C.steel)
  fill(img,10,38,4,1,C.brassDark)
  if state=="shine" then
    fill(img,9,2,2,5,C.brassWhite); fill(img,8,18,2,10,C.brassWhite)
    hline(img,4,19,33,C.brassWhite); diamond(img,5,15,2,C.cream)
  elseif state=="low" then
    fill(img,14,4,3,6,Color{r=82,g=31,b=23,a=255})
    fill(img,14,19,4,12,Color{r=79,g=49,b=19,a=255})
  end
  return img
end

local bullet=bulletImage("idle")
local bulletShine=bulletImage("shine")
local bulletLow=bulletImage("low")
saveBar(bullet,"bar_shop_bullet_western_brass_24x40_0_3_9.png")
saveBar(bulletShine,"bar_shop_bullet_western_brass_shine_24x40_0_3_9.png")
saveBar(bulletLow,"bar_shop_bullet_western_brass_low_24x40_0_3_9.png")

local bulletSource=Sprite(24,40,ColorMode.RGB); bulletSource.layers[1].name="western_brass_idle_shine_low"
setImage(bulletSource,bullet); bulletSource.frames[1].duration=0.24
for _,img in ipairs({bulletShine,bulletLow}) do
  local f=bulletSource:newEmptyFrame(); bulletSource:newCel(bulletSource.layers[1],f,img,Point(0,0)); f.duration=0.24
end
bulletSource:saveAs(sourceDir.."/bar_shop_bullet_western_brass_states_0_3_9.aseprite"); bulletSource:close()

-- Bullet flip and glint, retaining the existing 8x64 integration contract.
local flipFrames={}; local flipSheet=Image(512,64,ColorMode.RGB); flipSheet:clear(C.transparent)
for i=0,7 do
  local src=(i==3 or i==4) and bulletShine or bullet
  local fr=rotate(src,i*math.pi/4,64)
  if i==3 then
    hline(fr,12,52,32,C.brassHi); vline(fr,32,12,52,C.brassHi); diamond(fr,32,32,5,C.cream)
  end
  flipFrames[i+1]=fr; flipSheet:drawImage(fr,Point(i*64,0))
end
saveBar(flipSheet,"bar_shop_bullet_coin_flip_glint_8f_512x64_0_3_9.png")

local flipSource=Sprite(64,64,ColorMode.RGB); flipSource.layers[1].name="western_bullet_flip_glint_8f"
setImage(flipSource,flipFrames[1]); flipSource.frames[1].duration=0.07
for i=2,8 do local f=flipSource:newEmptyFrame(); flipSource:newCel(flipSource.layers[1],f,flipFrames[i],Point(0,0)); f.duration=(i==4) and 0.10 or 0.07 end
flipSource:saveAs(sourceDir.."/bar_shop_bullet_coin_flip_glint_0_3_9.aseprite"); flipSource:close()

-- 3+ price pour sheet with non-grid landing positions and rotations.
local pouch=resize(load(pouchPath),112,92)
local pouchAngles={0,-10,-24,-40,-50,-42,-24,-8}
local bulletPos={
  {}, {{56,56,1}}, {{48,62,2},{62,70,4}},
  {{39,72,3},{55,81,5},{72,86,6}},
  {{25,89,4},{48,95,6},{67,81,2},{89,96,7}},
  {{18,98,5},{43,83,7},{70,101,3},{96,87,1},{116,97,6}},
  {{16,101,6},{45,87,2},{72,103,7},{99,92,3},{124,100,5}},
  {{17,101,7},{49,88,3},{68,103,1},{101,92,5},{126,101,2}}
}
local pourFrames={}; local pourSheet=Image(1280,120,ColorMode.RGB); pourSheet:clear(C.transparent)
for i=1,8 do
  local fr=Image(160,120,ColorMode.RGB); fr:clear(C.transparent)
  if i<8 then fr:drawImage(rotate(pouch,pouchAngles[i]*math.pi/180,132),Point(42,2+(i-1)*2)) end
  for _,v in ipairs(bulletPos[i]) do fr:drawImage(resize(flipFrames[v[3]],24,24),Point(v[1],v[2])) end
  if i==5 then diamond(fr,66,87,3,C.cream) end
  pourFrames[i]=fr; pourSheet:drawImage(fr,Point((i-1)*160,0))
end
saveBar(pourSheet,"bar_shop_bullet_pour_table_8f_1280x120_0_3_9.png")
local pourSource=Sprite(160,120,ColorMode.RGB); pourSource.layers[1].name="western_bullet_pour_irregular_8f"
setImage(pourSource,pourFrames[1]); pourSource.frames[1].duration=0.09
for i=2,8 do local f=pourSource:newEmptyFrame(); pourSource:newCel(pourSource.layers[1],f,pourFrames[i],Point(0,0)); f.duration=(i>=7) and 0.14 or 0.09 end
pourSource:saveAs(sourceDir.."/bar_shop_bullet_pour_table_0_3_9.aseprite"); pourSource:close()

local function makeRopeBody()
  local img=Image(258,16,ColorMode.RGB); img:clear(C.transparent)
  -- Thick irregular silhouette. Light values are deliberate because runtime tinting darkens the body.
  for x=4,252 do
    local cy=7+(((x%47)==0 or (x%71)==0) and 1 or 0)
    local half=((x%31)==0) and 6 or 5
    for y=cy-half,cy+half do
      local edge=(y==cy-half or y==cy+half)
      local color=edge and C.ropeShadow or C.rope
      if y==cy-half+2 then color=C.ropeHi end
      if y>=cy+half-2 then color=C.ropeDark end
      img:drawPixel(x,y,color)
    end
  end

  -- Broad one-direction twist bands stay readable without becoming a lattice or progress-bar blocks.
  for x=-8,244,14 do
    line(img,x,2,x+9,13,C.ropeShadow)
    line(img,x+1,2,x+10,13,C.ropeDark)
    line(img,x+4,2,x+10,9,C.ropeHi)
    line(img,x+5,3,x+11,10,C.ropeHi)
    line(img,x+3,12,x+7,8,C.rope)
  end

  -- Knots, loose fibers and repaired wraps prevent a progress-bar silhouette.
  line(img,39,3,46,0,C.ropeHi); line(img,74,12,82,15,C.ropeShadow)
  line(img,132,3,139,0,C.ropeDark); line(img,188,12,196,15,C.ropeHi)
  fill(img,94,2,3,12,C.ropeShadow); fill(img,95,3,1,10,C.ropeHi)
  fill(img,178,2,3,12,C.ropeShadow); fill(img,179,3,1,10,C.ropeHi)

  -- Charred fuse tip is baked into the body because the current runtime only binds body + flame.
  for x=230,253 do
    local taper=math.min(5,math.max(2,5-math.floor((x-230)/8)))
    local color=(x<238) and C.smokeDark or C.ink
    for y=7-taper,7+taper do img:drawPixel(x,y,color) end
  end
  line(img,228,3,238,7,C.ropeDark); line(img,228,12,239,8,C.ropeShadow)
  fill(img,248,5,5,6,C.ember); fill(img,251,6,4,4,C.orange); diamond(img,255,8,1,C.cream)

  -- Frayed fibers at both ends.
  line(img,2,4,8,7,C.ropeHi); line(img,0,8,8,8,C.ropeDark); line(img,2,13,9,9,C.rope)
  line(img,251,3,257,0,C.smokeDark); line(img,252,8,257,8,C.ember); line(img,251,12,257,15,C.smoke)
  return img
end

local ropeBody=makeRopeBody(); saveHalli(ropeBody,"halli_rope_braided_body_258x16_0_3_9.png")
local ropeSource=Sprite(258,16,ColorMode.RGB); ropeSource.layers[1].name="braided_rope_uv_crop_not_stretch"
setImage(ropeSource,ropeBody); ropeSource:saveAs(sourceDir.."/halli_rope_braided_body_0_3_9.aseprite"); ropeSource:close()

local function makeCharCap()
  local img=Image(24,16,ColorMode.RGB); img:clear(C.transparent)
  -- Thick soot-black end with a narrow ember core and flying fibers.
  fill(img,0,2,15,12,C.smokeDark); fill(img,1,4,16,8,C.ink)
  line(img,0,3,15,7,C.ropeDark); line(img,0,12,15,8,C.ropeShadow)
  fill(img,14,4,6,8,C.ember); fill(img,17,5,5,6,C.orange)
  fill(img,20,6,3,4,C.yellow); diamond(img,23,8,1,C.cream)
  line(img,13,3,20,0,C.smoke); line(img,15,13,22,15,C.smokeDark)
  return img
end

local charCap=makeCharCap(); saveHalli(charCap,"halli_rope_burn_char_cap_24x16_0_3_9.png")
local charSource=Sprite(24,16,ColorMode.RGB); charSource.layers[1].name="charred_fibers_ember_cap"
setImage(charSource,charCap); charSource:saveAs(sourceDir.."/halli_rope_burn_char_cap_0_3_9.aseprite"); charSource:close()

local function makeFlame(frame)
  local img=Image(32,32,ColorMode.RGB); img:clear(C.transparent)
  -- The runtime centers this sprite on the shrinking rope end. Keep the
  -- blackened contact point at (16,16), then lick the flame back over the rope.
  local tipX={5,7,4,8,3,6}; local tipY={8,5,10,7,6,4}
  local lobeX={9,11,7,12,8,10}; local lobeY={13,10,14,12,11,9}
  disk(img,16,16,8,Color{r=255,g=73,b=8,a=42})
  disk(img,14,17,6,C.smokeDark); disk(img,17,16,4,C.ink)
  line(img,7,17,18,17,C.ropeShadow); line(img,9,15,17,15,C.ropeDark)
  triangle(img,18,11,19,22,tipX[frame],tipY[frame],C.ember)
  triangle(img,17,12,19,20,lobeX[frame],lobeY[frame],C.orange)
  disk(img,14+(frame%2),16,4,C.orange)
  triangle(img,17,13,18,20,11+(frame%3),11,C.yellow)
  diamond(img,17,16,2,C.cream); diamond(img,18,15,1,Color{r=255,g=255,b=226,a=255})
  -- Uneven sparks and a short smoke curl avoid the old candle-icon silhouette.
  diamond(img,5+(frame*3)%11,5+(frame%4),1,(frame%2==0) and C.brassHi or C.ember)
  diamond(img,21+(frame%3)*2,13-(frame%3),1,C.orange)
  if frame==2 or frame==5 then diamond(img,26,8,1,C.brassHi) end
  line(img,19,11,22+(frame%2),8,C.smoke)
  line(img,22+(frame%2),8,20+(frame%3),5,C.smokeDark)
  fill(img,20+(frame%3),3,2,2,Color{r=76,g=61,b=49,a=190})
  return img
end

local flameFrames={}; local flameSheet=Image(192,32,ColorMode.RGB); flameSheet:clear(C.transparent)
for i=1,6 do flameFrames[i]=makeFlame(i); flameSheet:drawImage(flameFrames[i],Point((i-1)*32,0)) end
saveHalli(flameFrames[5],"halli_rope_burn_flame_32x32_0_3_9.png")
saveHalli(flameSheet,"halli_rope_burn_flame_6f_192x32_0_3_9.png")
local flameSource=Sprite(32,32,ColorMode.RGB); flameSource.layers[1].name="burn_tip_flame_smoke_6f"
setImage(flameSource,flameFrames[1]); flameSource.frames[1].duration=0.08
for i=2,6 do local f=flameSource:newEmptyFrame(); flameSource:newCel(flameSource.layers[1],f,flameFrames[i],Point(0,0)); f.duration=0.08 end
flameSource:saveAs(sourceDir.."/halli_rope_burn_flame_0_3_9.aseprite"); flameSource:close()

local function makeBurst(frame)
  local img=Image(64,64,ColorMode.RGB); img:clear(C.transparent)
  local radii={2,5,10,16,20,23,25,27}; local r=radii[frame]
  if frame<=5 then
    -- Irregular gunpowder flash: sharp star, no circular orange HUD ring.
    local ray=r+10
    for a=0,11 do
      local ang=a*math.pi/6+frame*0.09
      local short=(a%2==0) and ray or math.floor(ray*0.68)
      local x0=32+math.floor(math.cos(ang)*math.max(2,r*0.35))
      local y0=32+math.floor(math.sin(ang)*math.max(2,r*0.35))
      local x1=32+math.floor(math.cos(ang)*short)
      local y1=32+math.floor(math.sin(ang)*short)
      line(img,x0,y0,x1,y1,(a%3==0) and C.brassHi or C.ember)
    end
    diamond(img,32,32,math.max(2,r),C.orange)
    diamond(img,32,32,math.max(1,math.floor(r*0.58)),C.yellow)
    diamond(img,32,32,math.max(1,math.floor(r*0.28)),C.cream)
  end
  if frame>=4 then
    local grow=frame-4
    disk(img,19-grow,25-grow,4+grow,C.smokeDark)
    disk(img,39+grow,20-grow,5+grow,C.smoke)
    disk(img,46+grow,38+grow,4+grow,C.smokeDark)
    disk(img,28-grow,45+grow,5+grow,C.smoke)
    diamond(img,14-grow,39+grow,1,C.ember)
    diamond(img,51+grow,27-grow,1,C.brassHi)
  end
  return img
end

local burstFrames={}; local burstSheet=Image(512,64,ColorMode.RGB); burstSheet:clear(C.transparent)
for i=1,8 do burstFrames[i]=makeBurst(i); burstSheet:drawImage(burstFrames[i],Point((i-1)*64,0)) end
saveHalli(burstFrames[4],"halli_rope_timeout_burst_64x64_0_3_9.png")
saveHalli(burstSheet,"halli_rope_timeout_burst_8f_512x64_0_3_9.png")
local burstSource=Sprite(64,64,ColorMode.RGB); burstSource.layers[1].name="timeout_burst_smoke_8f"
setImage(burstSource,burstFrames[1]); burstSource.frames[1].duration=0.075
for i=2,8 do local f=burstSource:newEmptyFrame(); burstSource:newCel(burstSource.layers[1],f,burstFrames[i],Point(0,0)); f.duration=(i>=6) and 0.11 or 0.075 end
burstSource:saveAs(sourceDir.."/halli_rope_timeout_burst_0_3_9.aseprite"); burstSource:close()

-- Old/new bullet comparison at readable nearest-neighbor scale.
local oldBullet=load(oldBulletPath)
local compare=Image(960,360,ColorMode.RGB); compare:clear(C.panel)
local bullets={oldBullet,bullet,bulletShine,bulletLow}
for i=1,4 do
  local x=(i-1)*240; fill(compare,x+12,12,216,336,Color{r=20,g=16,b=13,a=255})
  fill(compare,x+16,16,208,4,C.brassDark); fill(compare,x+16,340,208,4,C.brassDeep)
  compare:drawImage(resize(bullets[i],144,240),Point(x+48,60))
end
save(compare,previewDir.."/bar_shop_bullet_visual_comparison_960x360_0_3_9.png")
save(compare,outputDir.."/bar_shop_bullet_visual_comparison_960x360_0_3_9.png")

-- Four-stage rope storyboard: normal, mid, urgent and timeout burst.
local bg=resize(load(halliBackgroundPath),480,270)
local board=Image(1920,540,ColorMode.RGB); board:clear(C.panel)
local ratios={1.0,0.66,0.33,0.0}
for i=1,4 do
  local stage=Image(bg)
  local ratio=ratios[i]
  local rx=166; local ry=72; local rw=129
  fill(stage,rx-4,ry-3,rw+8,14,Color{r=8,g=6,b=5,a=210})
  if ratio>0 then
    local srcWidth=math.max(1,math.floor(ropeBody.width*ratio))
    local visible=resize(crop(ropeBody,srcWidth,16),math.max(1,math.floor(rw*ratio)),8)
    stage:drawImage(visible,Point(rx,ry))
    stage:drawImage(resize(charCap,12,8),Point(rx+math.floor(rw*ratio)-11,ry))
    stage:drawImage(resize(flameFrames[i==3 and 5 or i],18,18),Point(rx+math.floor(rw*ratio)-9,ry-5))
  else
    stage:drawImage(resize(burstFrames[5],54,54),Point(rx+rw/2-27,ry-23))
  end
  board:drawImage(stage,Point((i-1)*480,0))

  local zoom=Image(480,270,ColorMode.RGB); zoom:clear(Color{r=18,g=13,b=10,a=255})
  fill(zoom,20,30,440,210,Color{r=30,g=20,b=13,a=255})
  if ratio>0 then
    local srcWidth=math.max(1,math.floor(ropeBody.width*ratio))
    local visible=crop(ropeBody,srcWidth,16)
    zoom:drawImage(visible,Point(105,127))
    zoom:drawImage(charCap,Point(105+srcWidth-22,127))
    zoom:drawImage(flameFrames[i==3 and 5 or i],Point(105+srcWidth-16,119))
  else
    zoom:drawImage(burstFrames[5],Point(208,103))
  end
  board:drawImage(zoom,Point((i-1)*480,270))
end
for x=478,1438,480 do fill(board,x,0,4,540,C.brassDark) end
fill(board,0,268,1920,4,C.brassDark)
save(board,previewDir.."/halli_rope_burn_storyboard_1920x540_0_3_9.png")
save(board,outputDir.."/halli_rope_burn_storyboard_1920x540_0_3_9.png")

print("Bullet and rope art 0.3.9 generated")
