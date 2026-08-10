-- Textless in-game currency and portrait prediction UI 0.4.0.
-- Uses the approved western bullet and silhouettes; adds no player-facing words.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local runtimeDir=assert(p.runtimeDir)
local bulletPath=assert(p.bullet)
local playerPath=assert(p.player)
local aiPath=assert(p.ai)

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=8,g=7,b=6,a=255},
  panel=Color{r=13,g=14,b=15,a=245}, panel2=Color{r=23,g=20,b=16,a=255},
  brassDark=Color{r=91,g=51,b=16,a=255}, brass=Color{r=181,g=111,b=29,a=255},
  brassHi=Color{r=245,g=187,b=69,a=255}, cream=Color{r=255,g=230,b=146,a=255},
  teal=Color{r=32,g=191,b=203,a=255}, tealHi=Color{r=82,g=238,b=244,a=255},
  red=Color{r=202,g=53,b=55,a=255}, redHi=Color{r=255,g=83,b=87,a=255},
  crack=Color{r=32,g=18,b=11,a=255}, sand=Color{r=223,g=153,b=53,a=255},
  dim=Color{r=71,g=58,b=42,a=255}, white=Color{r=242,g=234,b=210,a=255}
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
local function saveFinal(img,name)
  save(img,runtimeDir..'/'..name); save(img,outputDir..'/'..name)
end
local function frame(img,x,y,w,h,border,inner)
  fill(img,x,y,w,h,C.ink); fill(img,x+2,y+2,w-4,h-4,border); fill(img,x+4,y+4,w-8,h-8,inner)
  diamond(img,x+6,y+6,2,C.brassHi); diamond(img,x+w-7,y+6,2,C.brassHi)
  diamond(img,x+6,y+h-7,2,C.brassDark); diamond(img,x+w-7,y+h-7,2,C.brassDark)
end

local bullet=load(bulletPath)
local basic=Image(40,40,ColorMode.RGB); basic:clear(C.transparent); basic:drawImage(bullet,Point(8,0))
saveFinal(basic,'currency_basic_bullet_40x40_0_4_0.png')

local temporary=Image(basic); temporary:drawImage(bullet,Point(8,0))
-- Shape distinction: a dark crack cuts through the brass body.
line(temporary,17,16,22,21,C.crack); line(temporary,22,21,18,26,C.crack); line(temporary,18,26,23,31,C.crack)
hline(temporary,18,20,25,C.cream)
-- Hourglass badge remains readable without relying on color.
fill(temporary,25,24,13,14,C.ink); fill(temporary,27,26,9,10,C.panel2)
hline(temporary,27,35,26,C.brassHi); hline(temporary,27,35,35,C.brassHi)
line(temporary,28,27,34,34,C.brass); line(temporary,34,27,28,34,C.brass)
diamond(temporary,31,31,2,C.sand)
saveFinal(temporary,'currency_temporary_cracked_hourglass_40x40_0_4_0.png')

local price=resize(bullet,14,24); local priceIcon=Image(24,24,ColorMode.RGB); priceIcon:clear(C.transparent); priceIcon:drawImage(price,Point(5,0))
saveFinal(priceIcon,'shop_price_bullet_24x24_0_4_0.png')

local function currencyPanel(icon,border)
  local img=Image(112,52,ColorMode.RGB); img:clear(C.transparent); frame(img,0,0,112,52,border,C.panel)
  img:drawImage(icon,Point(8,6)); fill(img,54,13,46,26,C.ink); fill(img,56,15,42,22,C.panel2)
  hline(img,58,94,35,C.brassDark); return img
end
local battleBasic=currencyPanel(basic,C.brass)
local battleTemporary=currencyPanel(temporary,C.brassDark)
saveFinal(battleBasic,'battle_currency_basic_panel_112x52_0_4_0.png')
saveFinal(battleTemporary,'battle_currency_temporary_panel_112x52_0_4_0.png')

local dual=Image(240,64,ColorMode.RGB); dual:clear(C.transparent); frame(dual,0,0,240,64,C.brass,C.panel)
dual:drawImage(temporary,Point(10,12)); fill(dual,54,17,47,28,C.ink); fill(dual,56,19,43,24,C.panel2)
-- Consumption order is communicated by the fixed left-to-right arrow, not text.
line(dual,108,31,128,31,C.brass); line(dual,123,26,128,31,C.brassHi); line(dual,123,36,128,31,C.brassHi)
dual:drawImage(basic,Point(135,12)); fill(dual,179,17,47,28,C.ink); fill(dual,181,19,43,24,C.panel2)
saveFinal(dual,'shop_currency_dual_panel_240x64_0_4_0.png')

local warning=Image(24,24,ColorMode.RGB); warning:clear(C.transparent)
diamond(warning,12,12,11,C.ink); diamond(warning,12,12,9,C.brassHi); diamond(warning,12,12,6,C.panel2)
fill(warning,11,6,3,8,C.cream); fill(warning,11,16,3,3,C.cream)
saveFinal(warning,'shop_exit_warning_badge_24x24_0_4_0.png')

local warnSheet=Image(144,24,ColorMode.RGB); warnSheet:clear(C.transparent)
local warnFrames={}
for i=1,6 do
  local fr=Image(24,24,ColorMode.RGB); fr:clear(C.transparent)
  local offset=(i==2 and -2) or (i==3 and 2) or (i==4 and -1) or (i==5 and 1) or 0
  fr:drawImage(warning,Point(offset,0)); if i==4 then diamond(fr,20,4,2,C.cream) end
  warnFrames[i]=fr; warnSheet:drawImage(fr,Point((i-1)*24,0))
end
saveFinal(warnSheet,'shop_exit_warning_pulse_6f_144x24_0_4_0.png')

local expireSheet=Image(320,40,ColorMode.RGB); expireSheet:clear(C.transparent); local expireFrames={}
for i=1,8 do
  local fr=Image(40,40,ColorMode.RGB); fr:clear(C.transparent)
  if i<=4 then fr:drawImage(temporary,Point((i%2==0) and 1 or 0,0)) end
  local start=math.max(1,i-2)
  for s=1,start*3 do
    local x=9+((s*11+i*3)%24); local y=20+((s*7+i*4)%18)
    diamond(fr,x,y,((s+i)%3==0) and 1 or 0,(s%2==0) and C.sand or C.brassHi)
  end
  if i>=5 then fill(fr,8,5,24,math.max(0,8-i),Color{r=90,g=52,b=20,a=120}) end
  expireFrames[i]=fr; expireSheet:drawImage(fr,Point((i-1)*40,0))
end
saveFinal(expireSheet,'currency_temporary_expire_8f_320x40_0_4_0.png')

local player=resize(load(playerPath),56,56); local ai=resize(load(aiPath),56,56)
local function portraitCard(subject,side,state)
  local img=Image(88,88,ColorMode.RGB); img:clear(C.transparent)
  local base=(side=='player') and C.teal or C.red
  local hi=(side=='player') and C.tealHi or C.redHi
  local border=(state=='idle') and C.brassDark or ((state=='hover') and C.brassHi or hi)
  frame(img,0,0,88,88,border,C.panel)
  fill(img,12,12,64,64,C.ink); fill(img,15,15,58,58,C.panel2); img:drawImage(subject,Point(16,16))
  if state=='hover' then
    hline(img,20,68,9,hi); hline(img,20,68,78,base)
  elseif state=='selected' then
    diamond(img,8,44,4,hi); diamond(img,79,44,4,hi); diamond(img,44,8,4,C.cream)
    hline(img,16,72,82,hi)
  end
  return img
end
local portraitSets={}
for _,side in ipairs({'player','ai'}) do
  local subject=(side=='player') and player or ai; portraitSets[side]={}
  for _,state in ipairs({'idle','hover','selected'}) do
    local img=portraitCard(subject,side,state); portraitSets[side][state]=img
    saveFinal(img,'poker_predict_'..side..'_portrait_'..state..'_88_0_4_0.png')
  end
end

local currencySource=Sprite(40,40,ColorMode.RGB); currencySource.layers[1].name='basic_and_temporary_shape_distinction'
setImage(currencySource,basic); local f=currencySource:newEmptyFrame(); currencySource:newCel(currencySource.layers[1],f,temporary,Point(0,0))
currencySource:saveAs(sourceDir..'/currency_basic_temporary_states_0_4_0.aseprite'); currencySource:close()

local warningSource=Sprite(24,24,ColorMode.RGB); warningSource.layers[1].name='textless_exit_warning_6f'
setImage(warningSource,warnFrames[1]); warningSource.frames[1].duration=0.07
for i=2,6 do local wf=warningSource:newEmptyFrame(); warningSource:newCel(warningSource.layers[1],wf,warnFrames[i],Point(0,0)); wf.duration=0.07 end
warningSource:saveAs(sourceDir..'/shop_exit_warning_pulse_0_4_0.aseprite'); warningSource:close()

local expireSource=Sprite(40,40,ColorMode.RGB); expireSource.layers[1].name='temporary_currency_expire_8f'
setImage(expireSource,expireFrames[1]); expireSource.frames[1].duration=0.06
for i=2,8 do local ef=expireSource:newEmptyFrame(); expireSource:newCel(expireSource.layers[1],ef,expireFrames[i],Point(0,0)); ef.duration=(i>=6) and 0.10 or 0.06 end
expireSource:saveAs(sourceDir..'/currency_temporary_expire_0_4_0.aseprite'); expireSource:close()

local portraitSource=Sprite(88,88,ColorMode.RGB); portraitSource.layers[1].name='player_ai_idle_hover_selected'
setImage(portraitSource,portraitSets.player.idle)
for _,img in ipairs({portraitSets.player.hover,portraitSets.player.selected,portraitSets.ai.idle,portraitSets.ai.hover,portraitSets.ai.selected}) do
  local pf=portraitSource:newEmptyFrame(); portraitSource:newCel(portraitSource.layers[1],pf,img,Point(0,0))
end
portraitSource:saveAs(sourceDir..'/poker_prediction_portrait_states_0_4_0.aseprite'); portraitSource:close()

local uiSource=Sprite(240,64,ColorMode.RGB); uiSource.layers[1].name='shop_dual_currency_panel'
setImage(uiSource,dual); uiSource:saveAs(sourceDir..'/shop_currency_dual_panel_0_4_0.aseprite'); uiSource:close()

-- One review board: shop states above, portrait prediction states below; no words are embedded.
local board=Image(960,540,ColorMode.RGB); board:clear(Color{r=9,g=8,b=7,a=255})
fill(board,24,22,912,220,Color{r=18,g=15,b=12,a=255}); fill(board,24,266,912,250,Color{r=15,g=17,b=18,a=255})
board:drawImage(dual,Point(72,64)); board:drawImage(battleBasic,Point(388,70)); board:drawImage(battleTemporary,Point(532,70))
board:drawImage(resize(warnSheet,432,72),Point(72,152)); board:drawImage(resize(expireSheet,640,80),Point(296,152))
board:drawImage(resize(portraitSets.ai.idle,132,132),Point(142,322)); board:drawImage(resize(portraitSets.ai.selected,132,132),Point(290,322))
board:drawImage(resize(portraitSets.player.idle,132,132),Point(546,322)); board:drawImage(resize(portraitSets.player.selected,132,132),Point(694,322))
line(board,466,300,466,484,C.brassDark); diamond(board,466,392,8,C.brassHi)
save(board,previewDir..'/textless_currency_portrait_preview_960x540_0_4_0.png')
save(board,outputDir..'/textless_currency_portrait_preview_960x540_0_4_0.png')

print('Textless currency and portrait UI art 0.4.0 generated')
