-- Gameplay art bridge for design specification 0.1.2.
-- Aseprite 1.3.18.1 batch generator: item icons, inventory/prediction UI,
-- Halli reveal-history readability and Poker item-phase layouts.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local commonDir=assert(p.commonDir)
local halliDir=assert(p.halliDir)
local pokerDir=assert(p.pokerDir)
local halliBasePath=assert(p.halliBase)
local roundTablePath=assert(p.roundTable)
local cardBackPath=assert(p.cardBack)
local cratePath=assert(p.crate)
local crateEmptyPath=assert(p.crateEmpty)
local crateFilledPath=assert(p.crateFilled)
local playerHeartFilledPath=assert(p.playerHeartFilled)
local playerHeartEmptyPath=assert(p.playerHeartEmpty)
local aiHeartFilledPath=assert(p.aiHeartFilled)
local aiHeartEmptyPath=assert(p.aiHeartEmpty)
local predictWinPath=assert(p.predictWin)
local predictLosePath=assert(p.predictLose)
local cardPaths={
  assert(p.card1),assert(p.card2),assert(p.card3),assert(p.card4),
  assert(p.card5),assert(p.card6),assert(p.card7),assert(p.card8)
}

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=7,g=6,b=6,a=255},
  shadow=Color{r=16,g=10,b=7,a=255}, wood=Color{r=58,g=28,b=14,a=255},
  wood2=Color{r=88,g=43,b=19,a=255}, woodHi=Color{r=126,g=67,b=29,a=255},
  brassDark=Color{r=83,g=48,b=16,a=255}, brass=Color{r=181,g=108,b=30,a=255},
  brassHi=Color{r=246,g=190,b=66,a=255}, cream=Color{r=247,g=226,b=179,a=255},
  cyanDark=Color{r=8,g=75,b=82,a=255}, cyan=Color{r=33,g=221,b=221,a=255},
  redDark=Color{r=91,g=20,b=24,a=255}, red=Color{r=244,g=57,b=65,a=255},
  green=Color{r=38,g=100,b=69,a=255}, greenHi=Color{r=84,g=176,b=110,a=255},
  gray=Color{r=66,g=64,b=61,a=255}, glass=Color{r=138,g=210,b=194,a=255}
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
  t=t or 1; fill(img,x,y,w,t,c); fill(img,x,y+h-t,w,t,c)
  fill(img,x,y,t,h,c); fill(img,x+w-t,y,t,h,c)
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function disk(img,cx,cy,r,c)
  for yy=-r,r do local span=math.floor(math.sqrt(r*r-yy*yy)); hline(img,cx-span,cx+span,cy+yy,c) end
end
local function ring(img,cx,cy,r,t,c)
  disk(img,cx,cy,r,c); disk(img,cx,cy,r-t,C.transparent)
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function clamp(v) return math.max(0,math.min(255,math.floor(v+0.5))) end
local function warmSaloonLighting(src)
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
      local dx=(x-w*0.50)/(w*0.39); local dy=(y-h*0.60)/(h*0.36)
      local onTable=(dx*dx+dy*dy)<=1
      local mul=onTable and 1.20 or 1.34
      local addR=onTable and 5 or 15; local addG=onTable and 3 or 10; local addB=onTable and 1 or 6
      -- Two broad practical-light pools reveal the bar shelves and wall decor.
      local practical=pool(x,y,w*0.22,h*0.28,w*0.28,h*0.34,18)
        +pool(x,y,w*0.80,h*0.25,w*0.24,h*0.32,15)
      dst:drawPixel(x,y,app.pixelColor.rgba(
        clamp(r*mul+addR+practical),clamp(g*mul+addG+practical*0.68),
        clamp(b*mul+addB+practical*0.28),a))
    end
  end end
  return dst
end
local function rotate180(src)
  local dst=Image(src.width,src.height,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,src.height-1 do for x=0,src.width-1 do dst:drawPixel(src.width-1-x,src.height-1-y,src:getPixel(x,y)) end end
  return dst
end
local function setImage(spr,img)
  spr.cels[1].image:clear(C.transparent); spr.cels[1].image:drawImage(img,Point(0,0))
end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close()
end
local function saveRuntime(img,name,dir)
  save(img,dir.."/"..name); save(img,outputDir.."/"..name)
end

local function baseIcon()
  local img=Image(64,64,ColorMode.RGB); img:clear(C.transparent)
  disk(img,32,32,27,C.shadow); ring(img,32,32,27,3,C.brassDark)
  ring(img,32,32,23,2,C.brass); return img
end

local function iconReload()
  local img=baseIcon()
  ring(img,29,31,14,3,C.brassHi); disk(img,29,31,4,C.ink)
  local holes={{29,20},{39,26},{36,38},{22,40},{18,27}}
  for _,v in ipairs(holes) do disk(img,v[1],v[2],3,C.ink); disk(img,v[1],v[2],1,C.brass) end
  fill(img,45,18,5,22,C.brass); fill(img,46,15,3,4,C.brassHi); fill(img,44,40,7,4,C.cream)
  hline(img,39,49,47,C.cyan); diamond(img,39,47,3,C.cyan)
  return img
end

local function iconBottomDeal()
  local img=baseIcon()
  fill(img,17,20,31,23,C.ink); fill(img,20,18,31,23,C.cream); stroke(img,20,18,31,23,C.brassDark,2)
  fill(img,14,43,34,5,C.wood2); fill(img,12,48,25,5,C.woodHi)
  fill(img,35,38,17,4,C.red); diamond(img,52,40,4,C.red)
  fill(img,19,25,17,2,C.redDark); fill(img,19,30,21,2,C.redDark)
  return img
end

local function iconHype()
  local img=baseIcon()
  -- cowboy bust and brass speaking horn
  fill(img,14,19,27,4,C.ink); fill(img,19,14,17,7,C.wood2); fill(img,21,22,13,13,C.cream)
  fill(img,18,35,19,11,C.cyanDark); fill(img,14,46,27,4,C.cyan)
  fill(img,37,27,7,9,C.brassDark); diamond(img,50,31,10,C.brass); diamond(img,50,31,6,C.shadow)
  hline(img,45,55,18,C.brassHi); hline(img,46,58,14,C.brass); hline(img,45,57,48,C.brass)
  return img
end

local function iconHeal()
  local img=baseIcon()
  fill(img,25,13,15,7,C.brass); stroke(img,25,13,15,7,C.ink,2)
  fill(img,22,20,21,31,C.green); stroke(img,22,20,21,31,C.ink,3)
  fill(img,25,23,15,24,C.glass); fill(img,27,26,3,17,C.cream)
  disk(img,32,37,8,C.red); diamond(img,32,37,5,C.cream)
  fill(img,30,31,5,13,C.cream); fill(img,26,35,13,5,C.cream)
  return img
end

local icons={iconReload(),iconBottomDeal(),iconHype(),iconHeal()}
local iconNames={"item_reload_64_0_1_2.png","item_bottom_deal_64_0_1_2.png","item_hype_man_64_0_1_2.png","item_heal_tonic_64_0_1_2.png"}
for i=1,4 do saveRuntime(icons[i],iconNames[i],commonDir) end
local iconSource=Sprite(64,64,ColorMode.RGB); iconSource.layers[1].name="IT01_IT02_IT03_HP"
setImage(iconSource,icons[1]); iconSource.frames[1].duration=0.2
for i=2,4 do local f=iconSource:newEmptyFrame(); iconSource:newCel(iconSource.layers[1],f,icons[i],Point(0,0)); f.duration=0.2 end
iconSource:saveAs(sourceDir.."/gameplay_item_icons_0_1_2.aseprite"); iconSource:close()
local iconSheet=Image(320,80,ColorMode.RGB); iconSheet:clear(C.ink)
for i=1,4 do iconSheet:drawImage(icons[i],Point(16+(i-1)*76,8)) end
save(iconSheet,previewDir.."/gameplay_item_icons_contact_sheet_320x80_0_1_2.png")
save(iconSheet,outputDir.."/gameplay_item_icons_contact_sheet_320x80_0_1_2.png")

local function itemSlot(state)
  local img=Image(72,72,ColorMode.RGB); img:clear(C.transparent)
  local edge=C.brassDark; local face=C.shadow
  if state=="hover" then edge=C.brassHi; face=C.wood
  elseif state=="selected" then edge=C.cyan; face=C.cyanDark
  elseif state=="disabled" then edge=C.gray; face=Color{r=23,g=22,b=21,a=255} end
  fill(img,5,7,62,60,C.ink); stroke(img,3,3,66,64,edge,3); fill(img,8,8,56,54,face)
  diamond(img,36,4,3,edge); diamond(img,36,66,3,edge)
  return img
end
local slotStates={itemSlot("idle"),itemSlot("hover"),itemSlot("selected"),itemSlot("disabled")}
local slotNames={"idle","hover","selected","disabled"}
for i=1,4 do saveRuntime(slotStates[i],"inventory_slot_72_"..slotNames[i].."_0_1_2.png",commonDir) end

local tray=Image(388,92,ColorMode.RGB); tray:clear(C.transparent)
fill(tray,4,10,380,76,C.ink); stroke(tray,2,6,384,80,C.brassDark,3); fill(tray,8,12,372,68,C.wood)
for i=1,4 do tray:drawImage(slotStates[1],Point(10+(i-1)*93,10)) end
diamond(tray,4,46,4,C.brass); diamond(tray,383,46,4,C.brass)
saveRuntime(tray,"inventory_tray_4slot_388x92_0_1_2.png",commonDir)
save(tray,pokerDir.."/poker_item_inventory_tray_388x92_0_3_6.png")
save(tray,outputDir.."/poker_item_inventory_tray_388x92_0_3_6.png")
local uiSource=Sprite(388,92,ColorMode.RGB); uiSource.layers[1].name="inventory_four_slot_tray"; setImage(uiSource,tray)
uiSource:saveAs(sourceDir.."/inventory_tray_4slot_0_1_2.aseprite"); uiSource:close()

local function predictionPip(filled)
  local img=Image(28,28,ColorMode.RGB); img:clear(C.transparent)
  fill(img,11,2,6,6,filled and C.brassHi or C.gray)
  fill(img,8,7,12,17,filled and C.brass or C.shadow)
  stroke(img,8,7,12,17,filled and C.brassHi or C.gray,2)
  fill(img,10,10,8,9,filled and C.cream or C.ink)
  return img
end
local pipEmpty=predictionPip(false); local pipFilled=predictionPip(true)
saveRuntime(pipEmpty,"prediction_success_pip_empty_28_0_1_2.png",commonDir)
saveRuntime(pipFilled,"prediction_success_pip_filled_28_0_1_2.png",commonDir)
local meter=Image(320,68,ColorMode.RGB); meter:clear(C.transparent)
fill(meter,5,8,310,54,C.shadow); stroke(meter,3,5,314,58,C.brassDark,3); fill(meter,10,12,300,46,C.wood)
for i=1,5 do meter:drawImage(i<=3 and pipFilled or pipEmpty,Point(78+(i-1)*42,20)) end
ring(meter,38,34,17,3,C.brass); disk(meter,38,34,5,C.brassHi)
saveRuntime(meter,"prediction_success_meter_320x68_0_1_2.png",commonDir)

local reward=Image(420,112,ColorMode.RGB); reward:clear(C.transparent)
fill(reward,5,8,410,98,C.shadow); stroke(reward,3,5,414,102,C.brassDark,3); fill(reward,10,12,400,90,C.wood)
for row=0,2 do
  local y=24+row*29; fill(reward,22,y,20,12,C.brass); fill(reward,26,y-4,12,5,C.brassHi)
  fill(reward,60,y-2,250,16,Color{r=20,g=14,b=10,a=255}); stroke(reward,60,y-2,250,16,C.brassDark,1)
end
diamond(reward,370,56,20,C.brass); diamond(reward,370,56,13,C.brassHi); diamond(reward,370,56,6,C.cream)
saveRuntime(reward,"prediction_reward_breakdown_panel_420x112_0_1_2.png",commonDir)

local function historyRail(color)
  local img=Image(72,122,ColorMode.RGB); img:clear(C.transparent)
  fill(img,3,16,66,101,Color{r=5,g=6,b=7,a=180}); stroke(img,1,14,70,105,color,2)
  for i=0,2 do fill(img,8+i*20,5,12,4,color) end
  return img
end
local playerRail=historyRail(C.cyan); local aiRail=historyRail(C.red)
saveRuntime(playerRail,"halli_reveal_history_rail_player_72x122_0_3_5.png",halliDir)
saveRuntime(aiRail,"halli_reveal_history_rail_ai_72x122_0_3_5.png",halliDir)

local cards={}; for i=1,8 do cards[i]=resize(load(cardPaths[i]),64,90) end
local function drawHistoryStack(img,x,y,rail,indices,count)
  img:drawImage(rail,Point(x-4,y-18))
  for i=1,count do img:drawImage(cards[indices[i]],Point(x,y+(i-1)*12)) end
end
local halli=load(halliBasePath)
drawHistoryStack(halli,214,190,playerRail,{1,2,3},3)
drawHistoryStack(halli,314,190,playerRail,{4,5,6},3)
drawHistoryStack(halli,582,190,aiRail,{7,8,1},3)
drawHistoryStack(halli,682,190,aiRail,{2,3,4},3)
save(halli,previewDir.."/halli_reveal_history_application_preview_960x540_0_3_5.png")
save(halli,outputDir.."/halli_reveal_history_application_preview_960x540_0_3_5.png")
local halliSource=Sprite(960,540,ColorMode.RGB); halliSource.layers[1].name="history_cards_previous_to_newest"; setImage(halliSource,halli)
halliSource:saveAs(sourceDir.."/halli_reveal_history_layout_0_3_5.aseprite"); halliSource:close()

local stackSheet=Image(480,140,ColorMode.RGB); stackSheet:clear(C.ink)
for state=0,3 do
  local cell=Image(120,140,ColorMode.RGB); cell:clear(C.shadow)
  if state>0 then drawHistoryStack(cell,28,12,playerRail,{1,2,3},state) end
  stackSheet:drawImage(cell,Point(state*120,0))
end
save(stackSheet,previewDir.."/halli_reveal_history_states_480x140_0_3_5.png")
save(stackSheet,outputDir.."/halli_reveal_history_states_480x140_0_3_5.png")

-- Poker wide layout. 0.1.2 removes the AI item slot because AI cannot use items.
local back=resize(load(cardBackPath),56,80); local backAI=rotate180(back)
local roundTableRaw=resize(load(roundTablePath),960,540)
local roundTable=warmSaloonLighting(roundTableRaw)
local lightingBoard=Image(1920,540,ColorMode.RGB); lightingBoard:clear(C.ink)
lightingBoard:drawImage(roundTableRaw,Point(0,0)); lightingBoard:drawImage(roundTable,Point(960,0))
fill(lightingBoard,957,0,6,540,C.brassHi)
save(roundTable,previewDir.."/saloon_lighting_visibility_reference_960x540_0_3_6.png")
save(roundTable,outputDir.."/saloon_lighting_visibility_reference_960x540_0_3_6.png")
save(lightingBoard,previewDir.."/saloon_lighting_before_after_1920x540_0_3_6.png")
save(lightingBoard,outputDir.."/saloon_lighting_before_after_1920x540_0_3_6.png")
local lightingSource=Sprite(960,540,ColorMode.RGB); lightingSource.layers[1].name="before_after_lighting_reference"
setImage(lightingSource,roundTableRaw); lightingSource.frames[1].duration=0.4
local lightingFrame=lightingSource:newEmptyFrame(); lightingSource:newCel(lightingSource.layers[1],lightingFrame,roundTable,Point(0,0)); lightingFrame.duration=0.4
lightingSource:saveAs(sourceDir.."/saloon_lighting_visibility_0_3_6.aseprite"); lightingSource:close()
local crateClosed=resize(load(cratePath),160,160)
local crateEmpty=resize(load(crateEmptyPath),180,180)
local crateFilled=resize(load(crateFilledPath),180,180)
local playerHeartFilled=resize(load(playerHeartFilledPath),24,24)
local playerHeartEmpty=resize(load(playerHeartEmptyPath),24,24)
local aiHeartFilled=resize(load(aiHeartFilledPath),24,24)
local aiHeartEmpty=resize(load(aiHeartEmptyPath),24,24)
local predictWin=resize(load(predictWinPath),64,64)
local predictLose=resize(load(predictLosePath),64,64)
local function pokerBase()
  local img=Image(roundTable)
  for i=0,2 do img:drawImage(backAI,Point(390+i*62,84)) end
  img:drawImage(cards[2],Point(420,211)); img:drawImage(cards[7],Point(488,211))
  img:drawImage(cards[4],Point(372,330)); img:drawImage(cards[6],Point(440,330)); img:drawImage(cards[8],Point(508,330))
  for i=0,2 do img:drawImage(i<2 and aiHeartFilled or aiHeartEmpty,Point(254+i*32,120)) end
  for i=0,2 do img:drawImage(playerHeartFilled,Point(254+i*32,366)) end
  img:drawImage(predictWin,Point(400,450)); img:drawImage(predictLose,Point(474,450))
  return img
end

local popup=Image(560,300,ColorMode.RGB); popup:clear(C.transparent)
fill(popup,8,12,544,280,Color{r=8,g=6,b=5,a=248}); stroke(popup,4,8,552,288,C.brassDark,4)
stroke(popup,12,16,536,272,C.woodHi,2); fill(popup,18,22,524,260,Color{r=25,g=15,b=10,a=245})
diamond(popup,18,22,5,C.brass); diamond(popup,541,22,5,C.brass)
diamond(popup,18,281,5,C.brass); diamond(popup,541,281,5,C.brass)
fill(popup,509,28,24,24,C.redDark); stroke(popup,509,28,24,24,C.brassDark,2)
for i=0,10 do fill(popup,515+i,34+i,2,2,C.red); fill(popup,525-i,34+i,2,2,C.red) end
save(popup,pokerDir.."/poker_item_popup_frame_560x300_0_3_6.png")
save(popup,outputDir.."/poker_item_popup_frame_560x300_0_3_6.png")

local function withDim(base)
  local img=Image(base)
  local dim=Image(960,540,ColorMode.RGB); dim:clear(C.transparent)
  fill(dim,0,0,960,540,Color{r=0,g=0,b=0,a=168}); img:drawImage(dim,Point(0,0))
  img:drawImage(popup,Point(200,120)); return img
end

local collapsed=pokerBase(); collapsed:drawImage(crateClosed,Point(670,310))
save(collapsed,previewDir.."/poker_item_box_closed_preview_960x540_0_3_6.png")
save(collapsed,outputDir.."/poker_item_box_closed_preview_960x540_0_3_6.png")

local emptyPopup=withDim(pokerBase()); emptyPopup:drawImage(crateEmpty,Point(390,180))
save(emptyPopup,previewDir.."/poker_item_box_open_empty_popup_preview_960x540_0_3_6.png")
save(emptyPopup,outputDir.."/poker_item_box_open_empty_popup_preview_960x540_0_3_6.png")

local filledPopup=withDim(pokerBase()); filledPopup:drawImage(crateFilled,Point(260,180))
local traySmall=resize(tray,292,69); filledPopup:drawImage(traySmall,Point(440,229))
for i=1,4 do filledPopup:drawImage(resize(icons[i],42,42),Point(454+(i-1)*69,242)) end
save(filledPopup,previewDir.."/poker_item_box_open_filled_popup_preview_960x540_0_3_6.png")
save(filledPopup,outputDir.."/poker_item_box_open_filled_popup_preview_960x540_0_3_6.png")

local pokerSource=Sprite(960,540,ColorMode.RGB); pokerSource.layers[1].name="closed_box"; setImage(pokerSource,collapsed)
local f2=pokerSource:newEmptyFrame(); pokerSource:newCel(pokerSource.layers[1],f2,emptyPopup,Point(0,0)); f2.duration=0.25
local f3=pokerSource:newEmptyFrame(); pokerSource:newCel(pokerSource.layers[1],f3,filledPopup,Point(0,0)); f3.duration=0.25
pokerSource:saveAs(sourceDir.."/poker_item_box_popup_states_0_3_6.aseprite"); pokerSource:close()

local rewardPreview=Image(960,540,ColorMode.RGB); rewardPreview:drawImage(roundTable,Point(0,0))
rewardPreview:drawImage(reward,Point(270,205)); rewardPreview:drawImage(meter,Point(320,126))
save(rewardPreview,previewDir.."/prediction_reward_result_preview_960x540_0_1_2.png")
save(rewardPreview,outputDir.."/prediction_reward_result_preview_960x540_0_1_2.png")

print("Gameplay 0.1.2 art bridge generated")
