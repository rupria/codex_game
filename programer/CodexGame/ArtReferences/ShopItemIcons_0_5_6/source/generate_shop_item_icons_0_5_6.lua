-- Shop item/currency UI 0.5.6 for GitHub issue #60.
-- Builds assets at the exact runtime draw sizes and demonstrates the approved 80px item art.

local p=app.params
local outDir=assert(p.outDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local backgroundPath=assert(p.background)

local C={
  clear=Color{r=0,g=0,b=0,a=0},
  black=Color{r=6,g=5,b=4,a=255}, panel=Color{r=15,g=12,b=9,a=255},
  panel2=Color{r=27,g=18,b=11,a=255}, leather=Color{r=55,g=30,b=16,a=255},
  brassDark=Color{r=78,g=43,b=13,a=255}, brass=Color{r=171,g=100,b=23,a=255},
  brassHi=Color{r=239,g=178,b=56,a=255}, copper=Color{r=186,g=73,b=28,a=255},
  steel=Color{r=91,g=94,b=91,a=255}, steelHi=Color{r=163,g=163,b=145,a=255},
  cream=Color{r=235,g=216,b=178,a=255}, teal=Color{r=40,g=211,b=202,a=255}
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
local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.clear)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end

local function iconPlate(accent,disabled)
  local img=Image(88,88,ColorMode.RGB); img:clear(C.clear)
  fill(img,0,0,88,88,C.black); fill(img,2,2,84,84,disabled and C.steel or C.brassDark)
  fill(img,4,4,80,80,C.panel); fill(img,8,8,72,72,C.panel2)
  hline(img,12,75,7,disabled and C.steelHi or accent)
  hline(img,12,75,80,C.brassDark); vline(img,7,12,75,C.leather); vline(img,80,12,75,C.leather)
  diamond(img,7,7,4,disabled and C.steelHi or accent); diamond(img,80,80,4,C.brass)
  return img
end

local plateIdle=iconPlate(C.brassHi,false)
local plateHover=iconPlate(C.teal,false)
local plateDisabled=iconPlate(C.steelHi,true)
save(plateIdle,outDir..'/bar_shop_item_icon_plate_idle_88x88_0_5_6.png')
save(plateHover,outDir..'/bar_shop_item_icon_plate_hover_88x88_0_5_6.png')
save(plateDisabled,outDir..'/bar_shop_item_icon_plate_disabled_88x88_0_5_6.png')

local function slotFrame()
  local img=Image(190,174,ColorMode.RGB); img:clear(C.clear)
  fill(img,0,0,190,174,C.black); fill(img,2,2,186,170,C.brassDark); fill(img,4,4,182,166,C.panel)
  fill(img,8,8,174,158,C.panel2)
  hline(img,12,177,107,C.brassDark); hline(img,12,177,135,C.brassDark)
  diamond(img,8,8,4,C.brassHi); diamond(img,181,8,4,C.brass)
  diamond(img,8,165,4,C.brass); diamond(img,181,165,4,C.brassHi)
  -- The runtime icon plate sits at (51, 10), item art at (55, 14), 80x80.
  img:drawImage(plateIdle,Point(51,10))
  return img
end
local slot=slotFrame()
save(slot,outDir..'/bar_shop_product_slot_190x174_0_5_6.png')
save(slot,sourceDir..'/bar_shop_item_layout_0_5_6.aseprite')

local function bulletIcon(size,temporary)
  local img=Image(size,size,ColorMode.RGB); img:clear(C.clear)
  local cx=math.floor(size/2)
  if not temporary then
    fill(img,cx-8,5,16,4,C.copper); fill(img,cx-10,9,20,9,C.copper)
    fill(img,cx-8,18,16,5,C.brassHi); fill(img,cx-9,23,18,16,C.brass)
    fill(img,cx-6,24,4,14,C.brassHi); fill(img,cx+5,24,3,14,C.brassDark)
    fill(img,cx-10,39,20,4,C.brassHi); fill(img,cx-8,43,16,2,C.brassDark)
  else
    fill(img,cx-8,5,16,4,C.steel); fill(img,cx-10,9,20,9,C.steelHi)
    fill(img,cx-8,18,16,5,C.brass); fill(img,cx-9,23,18,16,C.steel)
    fill(img,cx-6,24,4,14,C.steelHi); fill(img,cx+5,24,3,14,C.black)
    fill(img,cx-10,39,20,4,C.steelHi); fill(img,cx-8,43,16,2,C.black)
    -- Cracked/hourglass stamp distinguishes expiring ammunition at small sizes.
    hline(img,cx-5,cx+5,25,C.brassHi); hline(img,cx-4,cx+4,35,C.brassHi)
    for i=0,4 do img:drawPixel(cx-4+i,26+i,C.brassHi); img:drawPixel(cx+4-i,26+i,C.brassHi) end
    for i=0,4 do img:drawPixel(cx+i,30+i,C.brassHi); img:drawPixel(cx-i,30+i,C.brassHi) end
  end
  return img
end
local baseCurrency=bulletIcon(48,false)
local tempCurrency=bulletIcon(48,true)
save(baseCurrency,outDir..'/currency_basic_bullet_western_48_0_5_6.png')
save(tempCurrency,outDir..'/currency_temporary_cracked_round_48_0_5_6.png')

local price=resize(baseCurrency,28,28)
save(price,outDir..'/shop_price_bullet_western_28_0_5_6.png')

-- Full-screen layout preview. The item art is the already-approved 80px popup set.
local preview=resize(load(backgroundPath),960,540)
local xs={20,250,480,710}
local iconPaths={p.icon1,p.icon2,p.icon3,p.icon4}
for i=1,4 do
  local x=xs[i]
  preview:drawImage(slot,Point(x,146))
  local icon=load(assert(iconPaths[i]))
  preview:drawImage(icon,Point(x+55,160))
  fill(preview,x+30,263,130,2,C.cream)
  preview:drawImage(price,Point(x+61,280)); fill(preview,x+96,293,34,2,C.brassHi)
  fill(preview,x+24,284+28,142,28,C.black); fill(preview,x+26,314,138,24,C.leather)
  hline(preview,x+38,x+151,325,C.brassHi)
end
-- Top-left balance panels use the broader 48px silhouettes.
fill(preview,40,28,135,58,C.black); fill(preview,42,30,131,54,C.leather)
preview:drawImage(tempCurrency,Point(47,33)); fill(preview,104,56,48,3,C.cream)
fill(preview,183,28,135,58,C.black); fill(preview,185,30,131,54,C.leather)
preview:drawImage(baseCurrency,Point(190,33)); fill(preview,247,56,48,3,C.cream)
save(preview,previewDir..'/issue_60_shop_item_layout_preview_960x540_0_5_6.png')

local contact=Image(620,240,ColorMode.RGB); contact:clear(Color{r=8,g=7,b=6,a=255})
contact:drawImage(slot,Point(18,18)); contact:drawImage(plateIdle,Point(230,18)); contact:drawImage(plateHover,Point(334,18)); contact:drawImage(plateDisabled,Point(438,18))
contact:drawImage(baseCurrency,Point(250,136)); contact:drawImage(tempCurrency,Point(330,136)); contact:drawImage(price,Point(420,146))
save(contact,previewDir..'/shop_item_icon_assets_contact_sheet_620x240_0_5_6.png')

print('ShopItemIcons 0.5.6 review art generated')
