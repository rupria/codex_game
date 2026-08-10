-- Bar shop UI 0.3.4
-- Aseprite generator: iconless text-button skins, ammo pouch and purchase toss storyboard.

local p=app.params
local sourceImage=assert(p.pouchImage)
local bulletPath=assert(p.bullet)
local backgroundPath=assert(p.background)
local productSlotPath=assert(p.productSlot)
local ammoPanelPath=assert(p.ammoPanel)
local hpPanelPath=assert(p.hpPanel)
local itemPaths={assert(p.item1),assert(p.item2),assert(p.item3),assert(p.item4)}
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local runtimeDir=assert(p.runtimeDir)

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=5,g=4,b=3,a=255},
  shadow=Color{r=10,g=6,b=3,a=255}, wood=Color{r=55,g=25,b=12,a=255},
  wood2=Color{r=83,g=38,b=17,a=255}, woodHi=Color{r=119,g=55,b=22,a=255},
  brassDark=Color{r=82,g=46,b=13,a=255}, brass=Color{r=173,g=102,b=25,a=255},
  brassHi=Color{r=244,g=185,b=62,a=255}, cream=Color{r=246,g=225,b=178,a=255},
  disabled=Color{r=58,g=51,b=43,a=255}, cyan=Color{r=24,g=225,b=222,a=255}
}

local function load(path)
  local s=app.open(path); assert(s,"cannot open "..path); local i=Image(s.cels[1].image); s:close(); return i
end
local function alpha(pixel) return app.pixelColor.rgbaA(pixel) end
local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function stroke(img,x,y,w,h,c,t)
  t=t or 1; fill(img,x,y,w,t,c); fill(img,x,y+h-t,w,t,c); fill(img,x,y,t,h,c); fill(img,x+w-t,y,t,h,c)
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function clamp(v) return math.max(0,math.min(255,math.floor(v+0.5))) end
local function warmShopLighting(src)
  local dst=Image(src.width,src.height,ColorMode.RGB); dst:clear(C.transparent)
  local w=src.width; local h=src.height
  local function pool(x,y,cx,cy,rx,ry,power)
    local dx=(x-cx)/rx; local dy=(y-cy)/ry; local d=dx*dx+dy*dy
    if d>=1 then return 0 end
    return (1-d)*power
  end
  for y=0,h-1 do for x=0,w-1 do
    local px=src:getPixel(x,y); local a=app.pixelColor.rgbaA(px)
    if a>0 then
      local r=app.pixelColor.rgbaR(px); local g=app.pixelColor.rgbaG(px); local b=app.pixelColor.rgbaB(px)
      local onCounter=y>h*0.44
      local mul=onCounter and 1.20 or 1.36
      local addR=onCounter and 5 or 16; local addG=onCounter and 3 or 11; local addB=onCounter and 1 or 6
      local practical=pool(x,y,w*0.23,h*0.27,w*0.27,h*0.34,19)
        +pool(x,y,w*0.76,h*0.24,w*0.25,h*0.32,16)
      dst:drawPixel(x,y,app.pixelColor.rgba(
        clamp(r*mul+addR+practical),clamp(g*mul+addG+practical*0.68),
        clamp(b*mul+addB+practical*0.28),a))
    end
  end end
  return dst
end
local function tight(src)
  local x0=src.width-1; local y0=src.height-1; local x1=0; local y1=0; local found=false
  for y=0,src.height-1 do for x=0,src.width-1 do if alpha(src:getPixel(x,y))>8 then
    found=true; x0=math.min(x0,x); y0=math.min(y0,y); x1=math.max(x1,x); y1=math.max(y1,y)
  end end end
  assert(found,"empty pouch source")
  local out=Image(x1-x0+1,y1-y0+1,ColorMode.RGB); out:clear(C.transparent)
  for y=y0,y1 do for x=x0,x1 do out:drawPixel(x-x0,y-y0,src:getPixel(x,y)) end end
  return out
end
local function fit(src,w,h,pad)
  local s=math.min((w-pad*2)/src.width,(h-pad*2)/src.height)
  local nw=math.floor(src.width*s+0.5); local nh=math.floor(src.height*s+0.5)
  local out=Image(w,h,ColorMode.RGB); out:clear(C.transparent)
  out:drawImage(resize(src,nw,nh),Point(math.floor((w-nw)/2),h-pad-nh)); return out
end
local function setImage(spr,img) spr.cels[1].image:clear(C.transparent); spr.cels[1].image:drawImage(img,Point(0,0)) end
local function save(img,path) local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close() end

local function button(state)
  local img=Image(220,56,ColorMode.RGB); img:clear(C.transparent)
  local face=C.wood; local edge=C.brass; local hi=C.brassHi
  if state=="hover" then face=C.wood2; edge=C.brassHi
  elseif state=="pressed" then face=C.shadow; edge=C.brassDark; hi=C.brass
  elseif state=="disabled" then face=Color{r=24,g=20,b=17,a=255}; edge=C.disabled; hi=C.disabled end
  fill(img,7,8,206,43,C.shadow); stroke(img,5,5,210,45,C.ink,3)
  stroke(img,8,8,204,39,edge,2); fill(img,12,12,196,31,face)
  hline(img,16,203,14,hi); hline(img,16,203,40,C.brassDark)
  diamond(img,7,27,4,edge); diamond(img,212,27,4,edge)
  if state=="pressed" then fill(img,15,15,190,3,Color{r=31,g=14,b=8,a=255}) end
  return img
end

local font={
 A={"01110","10001","10001","11111","10001","10001","10001"},
 C={"01111","10000","10000","10000","10000","10000","01111"},
 E={"11111","10000","10000","11110","10000","10000","11111"},
 I={"11111","00100","00100","00100","00100","00100","11111"},
 L={"10000","10000","10000","10000","10000","10000","11111"},
 N={"10001","11001","10101","10101","10011","10001","10001"},
 O={"01110","10001","10001","10001","10001","10001","01110"},
 R={"11110","10001","10001","11110","10100","10010","10001"},
 T={"11111","00100","00100","00100","00100","00100","00100"},
 U={"10001","10001","10001","10001","10001","10001","01110"}
}
local function drawText(img,text,cx,y,scale,color)
  local total=#text*6*scale-scale; local ox=math.floor(cx-total/2)
  for i=1,#text do local g=font[text:sub(i,i)]
    if g then for yy=1,7 do for xx=1,5 do if g[yy]:sub(xx,xx)=="1" then fill(img,ox+(i-1)*6*scale+(xx-1)*scale,y+(yy-1)*scale,scale,scale,color) end end end end
  end
end

local function rotate(src,angle,size)
  local dst=Image(size,size,ColorMode.RGB); dst:clear(C.transparent)
  local ca=math.cos(angle); local sa=math.sin(angle); local cx=(src.width-1)/2; local cy=(src.height-1)/2; local dc=(size-1)/2
  for y=0,size-1 do for x=0,size-1 do
    local dx=x-dc; local dy=y-dc; local sx=math.floor(ca*dx+sa*dy+cx+0.5); local sy=math.floor(-sa*dx+ca*dy+cy+0.5)
    if sx>=0 and sy>=0 and sx<src.width and sy<src.height then dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end end
  return dst
end
local function spark(img,cx,cy)
  hline(img,cx-14,cx+14,cy,C.brassHi); fill(img,cx,cy-14,1,29,C.brassHi)
  diamond(img,cx,cy,5,C.cream); diamond(img,cx,cy,2,C.brassHi)
end

local pouchHi=tight(load(sourceImage)); local pouch=fit(pouchHi,180,150,4)
save(pouchHi,sourceDir.."/bar_shop_ammo_pouch_hires_0_3_4.png")
save(pouch,runtimeDir.."/bar_shop_ammo_pouch_180x150_0_3_4.png")
save(pouch,outputDir.."/bar_shop_ammo_pouch_180x150_0_3_4.png")

local states={button("idle"),button("hover"),button("pressed"),button("disabled")}
local stateNames={"idle","hover","pressed","disabled"}
for i=1,4 do
  save(states[i],runtimeDir.."/bar_shop_text_button_220x56_"..stateNames[i].."_0_3_4.png")
  save(states[i],outputDir.."/bar_shop_text_button_220x56_"..stateNames[i].."_0_3_4.png")
  save(resize(states[i],180,56),runtimeDir.."/bar_shop_reroll_"..stateNames[i].."_180x56_0_3_4.png")
  save(resize(states[i],180,56),outputDir.."/bar_shop_reroll_"..stateNames[i].."_180x56_0_3_4.png")
  if i<=3 then
    save(resize(states[i],200,56),runtimeDir.."/bar_shop_continue_"..stateNames[i].."_200x56_0_3_4.png")
    save(resize(states[i],200,56),outputDir.."/bar_shop_continue_"..stateNames[i].."_200x56_0_3_4.png")
  end
end
local bs=Sprite(220,56,ColorMode.RGB); bs.layers[1].name="iconless_localized_text_button_states"
setImage(bs,states[1]); bs.frames[1].duration=0.2
for i=2,4 do local f=bs:newEmptyFrame(); bs:newCel(bs.layers[1],f,states[i],Point(0,0)); f.duration=0.2 end
bs:saveAs(sourceDir.."/bar_shop_text_button_states_0_3_4.aseprite"); bs:close()
local buttonSheet=Image(880,56,ColorMode.RGB); buttonSheet:clear(C.transparent)
for i=1,4 do buttonSheet:drawImage(states[i],Point((i-1)*220,0)) end
save(buttonSheet,previewDir.."/bar_shop_text_button_states_880x56_0_3_4.png")
save(buttonSheet,outputDir.."/bar_shop_text_button_states_880x56_0_3_4.png")

local bullet=load(bulletPath); local spins={}
local spinSheet=Image(384,64,ColorMode.RGB); spinSheet:clear(C.transparent)
for i=0,5 do spins[i+1]=rotate(bullet,i*math.pi/3,64); spinSheet:drawImage(spins[i+1],Point(i*64,0)) end
save(spinSheet,runtimeDir.."/bar_shop_bullet_toss_spin_384x64_0_3_4.png")
save(spinSheet,outputDir.."/bar_shop_bullet_toss_spin_384x64_0_3_4.png")
local ss=Sprite(64,64,ColorMode.RGB); ss.layers[1].name="revolver_round_clockwise_toss"
setImage(ss,spins[1]); ss.frames[1].duration=0.08
for i=2,6 do local f=ss:newEmptyFrame(); ss:newCel(ss.layers[1],f,spins[i],Point(0,0)); f.duration=0.08 end
ss:saveAs(sourceDir.."/bar_shop_bullet_toss_spin_0_3_4.aseprite"); ss:close()

-- Build the presentation on the clean saloon background. This avoids the opaque
-- patch that used to sit behind the pouch in the screenshot-derived mockup.
local rawBackground=resize(load(backgroundPath),960,540)
local litBackground=warmShopLighting(rawBackground)
local lightingBoard=Image(1920,540,ColorMode.RGB); lightingBoard:clear(C.ink)
lightingBoard:drawImage(rawBackground,Point(0,0)); lightingBoard:drawImage(litBackground,Point(960,0))
fill(lightingBoard,957,0,6,540,C.brassHi)
save(litBackground,previewDir.."/bar_shop_lighting_visibility_reference_960x540_0_3_6.png")
save(litBackground,outputDir.."/bar_shop_lighting_visibility_reference_960x540_0_3_6.png")
save(lightingBoard,previewDir.."/bar_shop_lighting_before_after_1920x540_0_3_6.png")
save(lightingBoard,outputDir.."/bar_shop_lighting_before_after_1920x540_0_3_6.png")
local screen=Image(litBackground)
screen:drawImage(load(ammoPanelPath),Point(40,28))
screen:drawImage(load(hpPanelPath),Point(720,28))

-- 0.1.2 shop capacity: four products. Keep the whole row above the pouch zone.
local productSlot=resize(load(productSlotPath),190,174)
local productXs={20,250,480,710}
local productY=146
for i=1,4 do
  local x=productXs[i]
  screen:drawImage(productSlot,Point(x,productY))
  screen:drawImage(resize(load(itemPaths[i]),54,54),Point(x+68,productY+18))
end

-- The actual runtime uses localized TMP text over these iconless skins.
local left=resize(states[1],190,48); local right=resize(states[1],210,48)
screen:drawImage(left,Point(38,462)); screen:drawImage(right,Point(382,462))
drawText(screen,"REROLL",133,475,2,C.cream); drawText(screen,"CONTINUE",487,475,2,C.cream)
-- Transparent pouch, isolated at bottom-right with no frame and no product overlap.
screen:drawImage(pouch,Point(776,384))
save(screen,previewDir.."/bar_shop_text_buttons_and_pouch_preview_960x540_0_3_4.png")
save(screen,outputDir.."/bar_shop_text_buttons_and_pouch_preview_960x540_0_3_4.png")
save(screen,previewDir.."/bar_shop_four_slot_pouch_layout_preview_960x540_0_3_4.png")
save(screen,outputDir.."/bar_shop_four_slot_pouch_layout_preview_960x540_0_3_4.png")
local layout=Sprite(960,540,ColorMode.RGB); layout.layers[1].name="clean_saloon_four_slot_pouch_layout"
setImage(layout,screen); layout:saveAs(sourceDir.."/bar_shop_four_slot_pouch_layout_0_3_4.aseprite"); layout:close()

-- Four-step purchase motion storyboard: ready, launch, apex, shopkeeper contact.
local small=resize(screen,480,270); local board=Image(1920,270,ColorMode.RGB); board:clear(C.ink)
local pts={{435,231},{382,190},{306,116},{247,91}}
for i=1,4 do
  local f=Image(small)
  if i>1 then
    local b=resize(spins[i],32,32); f:drawImage(b,Point(pts[i][1]-16,pts[i][2]-16))
    for j=2,i-1 do diamond(f,pts[j][1],pts[j][2],2,C.brassHi) end
  end
  if i==4 then spark(f,pts[i][1],pts[i][2]) end
  board:drawImage(f,Point((i-1)*480,0))
  if i<4 then fill(board,i*480-2,0,4,270,C.brassDark) end
end
save(board,previewDir.."/bar_shop_bullet_purchase_storyboard_1920x270_0_3_4.png")
save(board,outputDir.."/bar_shop_bullet_purchase_storyboard_1920x270_0_3_4.png")
print("BarShop 0.3.4 generated")
