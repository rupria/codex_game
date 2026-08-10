-- Exact-size art for the Economy_0_1_2 file-replacement hooks.
-- No player-facing words are embedded in these sprites.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local runtimeDir=assert(p.runtimeDir)

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=8,g=7,b=6,a=255},
  panel=Color{r=14,g=14,b=14,a=248}, panel2=Color{r=27,g=22,b=16,a=255},
  brassDeep=Color{r=73,g=42,b=13,a=255}, brassDark=Color{r=119,g=68,b=18,a=255},
  brass=Color{r=190,g=118,b=31,a=255}, brassHi=Color{r=249,g=194,b=73,a=255},
  brassWhite=Color{r=255,g=235,b=156,a=255}, copperDeep=Color{r=75,g=30,b=18,a=255},
  copper=Color{r=154,g=67,b=31,a=255}, copperHi=Color{r=232,g=127,b=55,a=255},
  crack=Color{r=31,g=17,b=10,a=255}, sand=Color{r=228,g=157,b=51,a=255},
  teal=Color{r=34,g=203,b=211,a=255}, red=Color{r=232,g=61,b=65,a=255},
  cream=Color{r=255,g=244,b=184,a=255}
}

local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
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
  for yy=-r,r do local span=r-math.abs(yy); hline(img,cx-span,cx+span,cy+yy,c) end
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function setImage(spr,img)
  spr.cels[1].image:clear(C.transparent); spr.cels[1].image:drawImage(img,Point(0,0))
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close()
end
local function frame(img,x,y,w,h,accent)
  fill(img,x,y,w,h,C.ink); fill(img,x+2,y+2,w-4,h-4,C.brassDark)
  fill(img,x+4,y+4,w-8,h-8,C.panel); hline(img,x+8,x+w-9,y+5,accent)
  hline(img,x+8,x+w-9,y+h-6,C.brassDeep)
  diamond(img,x+7,y+7,2,C.brassHi); diamond(img,x+w-8,y+7,2,C.brassHi)
  diamond(img,x+7,y+h-8,2,C.brassDark); diamond(img,x+w-8,y+h-8,2,C.brassDark)
end

local function basic40()
  local img=Image(40,40,ColorMode.RGB); img:clear(C.transparent)
  -- Approved 0.3.9 geometry centered in a 40px hook canvas.
  fill(img,18,0,4,1,C.ink); fill(img,16,1,8,1,C.ink); fill(img,15,2,10,2,C.ink)
  fill(img,14,4,12,8,C.ink); fill(img,15,12,10,3,C.ink)
  fill(img,13,15,14,18,C.ink); fill(img,11,33,18,5,C.ink); fill(img,14,38,12,2,C.ink)
  fill(img,18,1,4,1,C.copperHi); fill(img,16,2,8,2,C.copper); fill(img,15,4,10,6,C.copper)
  fill(img,16,4,3,5,C.copperHi); fill(img,23,5,2,5,C.copperDeep); fill(img,15,10,10,2,C.copperDeep)
  fill(img,15,14,10,3,C.brassDark); fill(img,14,17,12,15,C.brass)
  fill(img,15,18,3,12,C.brassHi); fill(img,18,18,2,12,C.brassWhite); fill(img,23,18,3,13,C.brassDeep)
  fill(img,13,31,14,2,C.brassDeep); fill(img,12,33,16,4,C.brass); hline(img,12,27,33,C.brassWhite)
  fill(img,14,37,12,1,C.brassDeep); fill(img,16,38,8,1,C.brassDark)
  return img
end

local basic40img=basic40()
local temporary40=Image(basic40img)
line(temporary40,17,16,22,21,C.crack); line(temporary40,22,21,18,26,C.crack); line(temporary40,18,26,23,31,C.crack)
hline(temporary40,18,20,25,C.cream)
fill(temporary40,25,24,13,14,C.ink); fill(temporary40,27,26,9,10,C.panel2)
hline(temporary40,27,35,26,C.brassHi); hline(temporary40,27,35,35,C.brassHi)
line(temporary40,28,27,34,34,C.brass); line(temporary40,34,27,28,34,C.brass); diamond(temporary40,31,31,2,C.sand)

local basic48=Image(48,48,ColorMode.RGB); basic48:clear(C.transparent); basic48:drawImage(basic40img,Point(4,4))
local temp48=Image(48,48,ColorMode.RGB); temp48:clear(C.transparent); temp48:drawImage(temporary40,Point(4,4))
local price24=resize(basic40img,24,24)

local warning48=Image(48,48,ColorMode.RGB); warning48:clear(C.transparent)
diamond(warning48,24,24,22,C.ink); diamond(warning48,24,24,19,C.brassHi); diamond(warning48,24,24,14,C.panel2)
fill(warning48,21,12,7,20,C.cream); fill(warning48,21,36,7,6,C.cream)

local function countPanel(icon,temporary)
  local img=Image(160,58,ColorMode.RGB); img:clear(C.transparent); frame(img,0,0,160,58,temporary and C.sand or C.brassHi)
  img:drawImage(icon,Point(7,5)); fill(img,62,13,86,32,C.ink); fill(img,65,16,80,26,C.panel2)
  if temporary then
    line(img,68,18,76,40,C.brassDark); line(img,76,18,68,40,C.brassDark)
  else
    hline(img,69,141,40,C.brassDark)
  end
  return img
end
local basePanel=countPanel(basic48,false); local tempPanel=countPanel(temp48,true)

local function rewardFrame(icon,temporary)
  local img=Image(240,96,ColorMode.RGB); img:clear(C.transparent); frame(img,0,0,240,96,temporary and C.sand or C.brassHi)
  fill(img,12,18,64,64,C.ink); fill(img,15,21,58,58,C.panel2); img:drawImage(icon,Point(20,26))
  fill(img,88,25,132,48,C.ink); fill(img,92,29,124,40,C.panel2)
  if temporary then
    for i=0,4 do diamond(img,98+i*27,80-(i%2)*4,2,(i%2==0) and C.sand or C.brass) end
    line(img,96,33,106,65,C.brassDark); line(img,106,33,96,65,C.brassDark)
  else
    diamond(img,102,79,3,C.brassHi); hline(img,112,205,79,C.brassDark); diamond(img,214,79,3,C.brassHi)
  end
  return img
end
local baseReward=rewardFrame(basic48,false); local tempReward=rewardFrame(temp48,true)

local assets={
  {'currency_base_icon_48_0_1_2.png',basic48},
  {'currency_temporary_icon_48_0_1_2.png',temp48},
  {'currency_price_icon_24_0_1_2.png',price24},
  {'currency_base_panel_160x58_0_1_2.png',basePanel},
  {'currency_temporary_panel_160x58_0_1_2.png',tempPanel},
  {'stage_reward_base_frame_240x96_0_1_2.png',baseReward},
  {'stage_reward_temporary_frame_240x96_0_1_2.png',tempReward},
  {'shop_exit_warning_icon_48_0_1_2.png',warning48}
}
for _,v in ipairs(assets) do save(v[2],runtimeDir..'/'..v[1]) end

local iconSource=Sprite(48,48,ColorMode.RGB); iconSource.layers[1].name='base_temporary_exit_warning'
setImage(iconSource,basic48)
for _,img in ipairs({temp48,warning48}) do local f=iconSource:newEmptyFrame(); iconSource:newCel(iconSource.layers[1],f,img,Point(0,0)) end
iconSource:saveAs(sourceDir..'/economy_currency_icons_48_0_1_2.aseprite'); iconSource:close()

local panelSource=Sprite(160,58,ColorMode.RGB); panelSource.layers[1].name='base_and_temporary_panels'
setImage(panelSource,basePanel); local pf=panelSource:newEmptyFrame(); panelSource:newCel(panelSource.layers[1],pf,tempPanel,Point(0,0))
panelSource:saveAs(sourceDir..'/economy_currency_panels_160x58_0_1_2.aseprite'); panelSource:close()

local rewardSource=Sprite(240,96,ColorMode.RGB); rewardSource.layers[1].name='base_and_temporary_reward_frames'
setImage(rewardSource,baseReward); local rf=rewardSource:newEmptyFrame(); rewardSource:newCel(rewardSource.layers[1],rf,tempReward,Point(0,0))
rewardSource:saveAs(sourceDir..'/economy_reward_frames_240x96_0_1_2.aseprite'); rewardSource:close()

local board=Image(960,540,ColorMode.RGB); board:clear(Color{r=9,g=8,b=7,a=255})
fill(board,24,24,912,492,Color{r=18,g=15,b=12,a=255})
board:drawImage(resize(basic48,96,96),Point(72,66)); board:drawImage(resize(temp48,96,96),Point(192,66))
board:drawImage(basePanel,Point(340,86)); board:drawImage(tempPanel,Point(530,86)); board:drawImage(resize(warning48,96,96),Point(746,66))
board:drawImage(baseReward,Point(150,270)); board:drawImage(tempReward,Point(570,270))
-- Fixed shop consumption direction: temporary left to base right.
line(board,394,214,566,214,C.brass); line(board,554,204,566,214,C.brassHi); line(board,554,224,566,214,C.brassHi)
save(board,previewDir..'/economy_art_hooks_preview_960x540_0_1_2.png')

print('Economy_0_1_2 art slots generated')
