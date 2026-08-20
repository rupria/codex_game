-- Approved Three Call entry art for GitHub issue #72.
-- Required params: background, card, lock, seal, outputRoot

local p=app.params
local outputRoot=assert(p.outputRoot,"outputRoot is required")

local function openImage(path)
  local s=assert(app.open(path),"cannot open "..path)
  local im=s.cels[1].image:clone(); s:close(); return im
end
local background=openImage(assert(p.background,"background is required"))
local card=openImage(assert(p.card,"card is required"))
local lock=openImage(assert(p.lock,"lock is required"))
local seal=openImage(assert(p.seal,"seal is required"))

local function image(w,h)
  local im=Image(w,h,ColorMode.RGB); im:clear(Color{r=0,g=0,b=0,a=0}); return im
end
local function fill(im,x,y,w,h,c)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function h(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function v(im,x,y,hh,c,t) fill(im,x,y,t or 1,hh,c) end
local function frame(im,x,y,w,hh,c,t)
  t=t or 1; h(im,x,y,w,c,t); h(im,x,y+hh-t,w,c,t); v(im,x,y,hh,c,t); v(im,x+w-t,y,hh,c,t)
end
local function disc(im,cx,cy,r,c)
  for y=-r,r do local s=math.floor(math.sqrt(r*r-y*y)); h(im,cx-s,cy+y,s*2+1,c,1) end
end
local function diamond(im,cx,cy,r,c)
  for y=-r,r do local w=r-math.abs(y); h(im,cx-w,cy+y,w*2+1,c,1) end
end
local function scaled(src,w,hh)
  local out=image(w,hh)
  for y=0,hh-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/hh))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      out:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return out
end

local C={
  ink=Color{r=7,g=6,b=5,a=255}, shadow=Color{r=0,g=0,b=0,a=188},
  leather=Color{r=24,g=16,b=12,a=255}, leather2=Color{r=42,g=27,b=17,a=255},
  brass0=Color{r=71,g=41,b=16,a=255}, brass1=Color{r=129,g=77,b=25,a=255},
  brass2=Color{r=204,g=139,b=44,a=255}, brass3=Color{r=246,g=199,b=92,a=255},
  cyan=Color{r=53,g=203,b=193,a=255}, red=Color{r=220,g=61,b=50,a=255},
  cream=Color{r=236,g=224,b=191,a=255}
}

local function plaque()
  local im=image(380,112)
  fill(im,7,10,366,96,C.shadow)
  fill(im,1,5,378,100,C.ink); fill(im,4,8,372,94,C.brass0); fill(im,7,11,366,88,C.leather)
  frame(im,11,15,358,80,C.brass1,2)
  h(im,96,26,252,C.brass0,2); h(im,96,84,252,C.brass0,2)
  v(im,92,22,66,C.brass1,2)
  diamond(im,18,56,5,C.brass2); diamond(im,362,56,5,C.brass2)
  fill(im,20,20,3,3,C.brass3); fill(im,357,20,3,3,C.brass3)
  fill(im,20,89,3,3,C.brass1); fill(im,357,89,3,3,C.brass1)
  h(im,112,77,222,C.brass1,1)
  return im
end

local glyph={
  A={"01110","10001","10001","11111","10001","10001","10001"},
  C={"01111","10000","10000","10000","10000","10000","01111"},
  E={"11111","10000","10000","11110","10000","10000","11111"},
  H={"10001","10001","10001","11111","10001","10001","10001"},
  L={"10000","10000","10000","10000","10000","10000","11111"},
  R={"11110","10001","10001","11110","10100","10010","10001"},
  T={"11111","00100","00100","00100","00100","00100","00100"}
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

local function pulseFrame(index)
  local im=image(64,64); im:drawImage(seal,Point(0,0))
  if index>=2 and index<=6 then
    local alpha=math.max(48,190-(index-2)*30)
    local c=Color{r=246,g=199,b=92,a=alpha}
    local r=23+(index-2)*3
    for yy=-r,r do
      local xx=math.floor(math.sqrt(math.max(0,r*r-yy*yy)))
      if math.abs(yy)%4<2 then
        if 32-xx>=0 then im:drawPixel(32-xx,32+yy,c) end
        if 32+xx<64 then im:drawPixel(32+xx,32+yy,c) end
      end
    end
  end
  if index==3 or index==4 then
    diamond(im,52,10,3,C.brass3); diamond(im,11,48,2,C.cream)
  end
  return im
end

local plaqueMaster=plaque()
plaqueMaster:saveAs(outputRoot.."/three_call_entry_center_plaque_380x112_0_6_0.png")

local pulseSheet=image(512,64); local pulses={}
for i=1,8 do pulses[i]=pulseFrame(i); pulseSheet:drawImage(pulses[i],Point((i-1)*64,0)) end
pulseSheet:saveAs(outputRoot.."/three_call_bell_pulse_8f_512x64_0_6_0.png")

local function screen(stage)
  local im=background:clone()
  im:drawImage(card,Point(424,42)); im:drawImage(lock,Point(510,61))
  local dim=image(960,540); fill(dim,0,0,960,540,Color{r=0,g=0,b=0,a=96}); im:drawImage(dim,Point(0,0))
  if stage==1 then
    local small=scaled(plaqueMaster,190,56); im:drawImage(small,Point(385,242))
    local s=scaled(pulses[1],40,40); im:drawImage(s,Point(397,250))
  elseif stage==2 then
    local mid=scaled(plaqueMaster,304,90); im:drawImage(mid,Point(328,225))
    local s=scaled(pulses[3],56,56); im:drawImage(s,Point(345,242))
    label(im,"THREE CALL",430,252,3,C.cream)
  elseif stage==3 then
    im:drawImage(plaqueMaster,Point(290,214)); im:drawImage(pulses[4],Point(307,238))
    label(im,"THREE CALL",405,246,4,C.cream)
  else
    local fade=image(380,112); fade:drawImage(plaqueMaster,Point(0,0)); fill(fade,0,0,380,112,Color{r=0,g=0,b=0,a=86})
    im:drawImage(fade,Point(290,214)); im:drawImage(pulses[7],Point(307,238))
    label(im,"THREE CALL",405,246,4,Color{r=156,g=142,b=115,a=255})
  end
  return im
end

local approved=screen(3)
approved:saveAs(outputRoot.."/three_call_entry_center_application_preview_960x540_0_6_0.png")

local storyboard=image(1920,270)
for i=1,4 do storyboard:drawImage(scaled(screen(i),480,270),Point((i-1)*480,0)) end
storyboard:saveAs(outputRoot.."/three_call_entry_timing_storyboard_1920x270_0_6_0.png")

local src=Sprite(380,112,ColorMode.RGB); src.layers[1].name="center_plaque_text_safe"
src.cels[1].image:drawImage(plaqueMaster,Point(0,0)); src:saveAs(outputRoot.."/three_call_entry_center_plaque_380x112_0_6_0.aseprite"); src:close()
local anim=Sprite(64,64,ColorMode.RGB); anim.layers[1].name="approved_bell_pulse"
anim.cels[1].image:drawImage(pulses[1],Point(0,0))
for i=2,8 do local f=anim:newEmptyFrame(); anim:newCel(anim.layers[1],f,pulses[i],Point(0,0)) end
anim:saveAs(outputRoot.."/three_call_bell_pulse_8f_64_0_6_0.aseprite"); anim:close()
