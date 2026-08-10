-- Halli two-card fan and Poker item-selection UI bridge 0.3.7.
-- Generated with Aseprite 1.3.x. No game-rule logic is changed here.

local p=app.params
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local halliDir=assert(p.halliDir)
local pokerDir=assert(p.pokerDir)
local halliBasePath=assert(p.halliBase)
local pokerBasePath=assert(p.pokerBase)
local crateEmptyPath=assert(p.crateEmpty)
local crateFilledPath=assert(p.crateFilled)
local cardPaths={assert(p.card1),assert(p.card2),assert(p.card3),assert(p.card4),assert(p.card5),assert(p.card6),assert(p.card7),assert(p.card8)}
local iconPaths={assert(p.icon1),assert(p.icon2),assert(p.icon3),assert(p.icon4)}
local targetCardPaths={assert(p.targetCard1),assert(p.targetCard2),assert(p.targetCard3)}

local C={
  transparent=Color{r=0,g=0,b=0,a=0},
  ink=Color{r=7,g=6,b=6,a=255},
  shadow=Color{r=16,g=10,b=7,a=255},
  panel=Color{r=27,g=16,b=10,a=248},
  panel2=Color{r=43,g=25,b=14,a=248},
  wood=Color{r=70,g=35,b=17,a=255},
  woodHi=Color{r=126,g=67,b=29,a=255},
  brassDark=Color{r=83,g=48,b=16,a=255},
  brass=Color{r=181,g=108,b=30,a=255},
  brassHi=Color{r=246,g=190,b=66,a=255},
  cream=Color{r=247,g=226,b=179,a=255},
  cyanDark=Color{r=8,g=75,b=82,a=255},
  cyan=Color{r=33,g=221,b=221,a=255},
  redDark=Color{r=91,g=20,b=24,a=255},
  red=Color{r=244,g=57,b=65,a=255},
  gray=Color{r=77,g=72,b=66,a=255}
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

local function setImage(spr,img)
  spr.cels[1].image:clear(C.transparent)
  spr.cels[1].image:drawImage(img,Point(0,0))
end

local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB)
  setImage(s,img); s:saveAs(path); s:close()
end

local function saveRuntime(img,name,dir)
  save(img,dir.."/"..name)
  save(img,outputDir.."/"..name)
end

local halliBase=resize(load(halliBasePath),960,540)
local pokerBase=resize(load(pokerBasePath),960,540)
local cards={}
for i=1,8 do cards[i]=resize(load(cardPaths[i]),76,106) end
local icons={}
for i=1,4 do icons[i]=resize(load(iconPaths[i]),52,52) end
local targetCards={}
for i=1,3 do targetCards[i]=resize(load(targetCardPaths[i]),56,78) end

-- Shared left/right pile rails. They have no opaque black rectangle.
local function sharedPileRail(accent)
  local img=Image(140,136,ColorMode.RGB); img:clear(C.transparent)
  fill(img,3,127,132,3,Color{r=0,g=0,b=0,a=110})
  fill(img,8,132,122,2,accent)
  diamond(img,4,133,3,accent); diamond(img,135,133,3,accent)
  return img
end

local idleRail=sharedPileRail(C.brass)
local playerRail=sharedPileRail(C.cyan)
local aiRail=sharedPileRail(C.red)
saveRuntime(idleRail,"halli_shared_pile_rail_idle_140x136_0_3_7.png",halliDir)
saveRuntime(playerRail,"halli_shared_pile_rail_player_active_140x136_0_3_7.png",halliDir)
saveRuntime(aiRail,"halli_shared_pile_rail_ai_active_140x136_0_3_7.png",halliDir)

local function drawSharedPile(img,x,y,rail,previousIndex,newIndex,count)
  if count<=0 then return end
  img:drawImage(rail,Point(x-2,y-2))
  if count==1 then
    img:drawImage(cards[previousIndex],Point(x+42,y))
    return
  end
  -- New card stays on the anchor. The previous card slides down-left and
  -- remains readable; rank, suit and skull art from both cards stay exposed.
  img:drawImage(cards[newIndex],Point(x+42,y))
  img:drawImage(cards[previousIndex],Point(x,y+22))
end

local function halliState(count)
  local img=Image(halliBase)
  -- Screen-left pile: Player reveal 1 -> AI reveal 2.
  drawSharedPile(img,250,183,count>=2 and aiRail or playerRail,1,2,count)
  -- Screen-right pile: AI reveal 1 -> Player reveal 2.
  drawSharedPile(img,610,183,count>=2 and playerRail or aiRail,5,6,count)
  return img
end

local halliTwo=halliState(2)
save(halliTwo,previewDir.."/halli_two_card_fan_application_preview_960x540_0_3_7.png")
save(halliTwo,outputDir.."/halli_two_card_fan_application_preview_960x540_0_3_7.png")

local fanStates=Image(480,160,ColorMode.RGB); fanStates:clear(C.ink)
for count=0,2 do
  local cell=Image(160,160,ColorMode.RGB); cell:clear(C.shadow)
  if count>0 then drawSharedPile(cell,10,13,count>=2 and aiRail or playerRail,1,2,count) end
  fanStates:drawImage(cell,Point(count*160,0))
end
save(fanStates,previewDir.."/halli_two_card_shared_pile_states_480x160_0_3_7.png")
save(fanStates,outputDir.."/halli_two_card_shared_pile_states_480x160_0_3_7.png")

local halliSource=Sprite(960,540,ColorMode.RGB)
halliSource.layers[1].name="left_right_shared_piles_0_1_2"
setImage(halliSource,halliState(0)); halliSource.frames[1].duration=0.25
for count=1,2 do
  local f=halliSource:newEmptyFrame()
  halliSource:newCel(halliSource.layers[1],f,halliState(count),Point(0,0))
  f.duration=0.25
end
halliSource:saveAs(sourceDir.."/halli_two_card_shared_pile_layout_0_3_7.aseprite")
halliSource:close()

-- Poker item selection modal and selected-item detail area.
local modal=Image(640,336,ColorMode.RGB); modal:clear(C.transparent)
fill(modal,8,12,624,316,Color{r=6,g=5,b=4,a=248})
stroke(modal,4,8,632,324,C.brassDark,4)
stroke(modal,12,16,616,308,C.woodHi,2)
fill(modal,18,22,604,296,C.panel)
diamond(modal,18,22,5,C.brass); diamond(modal,621,22,5,C.brass)
diamond(modal,18,317,5,C.brass); diamond(modal,621,317,5,C.brass)
saveRuntime(modal,"poker_item_select_panel_640x336_0_3_7.png",pokerDir)

local detail=Image(376,112,ColorMode.RGB); detail:clear(C.transparent)
fill(detail,4,6,368,102,C.shadow); stroke(detail,2,4,372,106,C.brassDark,3)
fill(detail,10,12,356,90,C.panel2)
stroke(detail,16,18,76,76,C.cyan,2)
fill(detail,106,23,236,10,C.cream)
fill(detail,106,45,248,7,C.woodHi)
fill(detail,106,61,218,7,C.woodHi)
fill(detail,106,77,188,7,C.brassDark)
diamond(detail,371,56,4,C.brass)
saveRuntime(detail,"poker_item_detail_panel_376x112_0_3_7.png",pokerDir)

local function actionButton(state)
  local img=Image(172,44,ColorMode.RGB); img:clear(C.transparent)
  local edge=C.brassDark; local face=C.wood; local line=C.cream
  if state=="hover" then edge=C.brassHi; face=C.woodHi
  elseif state=="disabled" then edge=C.gray; face=C.shadow; line=C.gray end
  fill(img,4,6,164,34,C.ink); stroke(img,2,4,168,38,edge,3)
  fill(img,9,10,154,26,face); fill(img,34,18,104,6,line)
  diamond(img,5,22,3,edge); diamond(img,166,22,3,edge)
  return img
end

local buttonIdle=actionButton("idle")
local buttonHover=actionButton("hover")
local buttonDisabled=actionButton("disabled")
saveRuntime(buttonIdle,"poker_item_action_button_idle_172x44_0_3_7.png",pokerDir)
saveRuntime(buttonHover,"poker_item_action_button_hover_172x44_0_3_7.png",pokerDir)
saveRuntime(buttonDisabled,"poker_item_action_button_disabled_172x44_0_3_7.png",pokerDir)

local function dim(base)
  local img=Image(base)
  local overlay=Image(960,540,ColorMode.RGB); overlay:clear(C.transparent)
  fill(overlay,0,0,960,540,Color{r=0,g=0,b=0,a=164})
  img:drawImage(overlay,Point(0,0)); return img
end

local crateEmpty=resize(load(crateEmptyPath),170,170)
local crateFilled=resize(load(crateFilledPath),170,170)

local function slot(state)
  local img=Image(68,68,ColorMode.RGB); img:clear(C.transparent)
  local edge=C.brassDark; local face=C.shadow
  if state=="selected" then edge=C.cyan; face=C.cyanDark
  elseif state=="disabled" then edge=C.gray; face=C.ink end
  fill(img,4,6,60,56,C.ink); stroke(img,2,4,64,60,edge,3); fill(img,8,10,52,48,face)
  diamond(img,34,4,3,edge); diamond(img,34,63,3,edge)
  return img
end

local slotIdle=slot("idle")
local slotSelected=slot("selected")
local slotDisabled=slot("disabled")

local function drawSlots(img,x,y,selected,disabled)
  for i=1,4 do
    local s=disabled and slotDisabled or (i==selected and slotSelected or slotIdle)
    img:drawImage(s,Point(x+(i-1)*78,y))
    if not disabled then img:drawImage(icons[i],Point(x+8+(i-1)*78,y+8)) end
  end
end

local function itemSelectedState(targetMode)
  local img=dim(pokerBase)
  img:drawImage(modal,Point(160,102))
  img:drawImage(crateFilled,Point(184,174))
  drawSlots(img,404,154,1,false)
  if not targetMode then
    img:drawImage(detail,Point(402,232))
    img:drawImage(icons[1],Point(420,254))
  else
    fill(img,404,235,372,118,C.shadow)
    stroke(img,402,233,376,122,C.brassDark,3)
    for i=1,3 do
      local x=470+(i-1)*66
      img:drawImage(targetCards[i],Point(x,252))
      if i==2 then stroke(img,x-3,249,62,84,C.cyan,3) end
    end
  end
  img:drawImage(targetMode and buttonHover or buttonIdle,Point(408,368))
  img:drawImage(buttonIdle,Point(596,368))
  return img
end

local function itemEmptyState()
  local img=dim(pokerBase)
  img:drawImage(modal,Point(160,102))
  img:drawImage(crateEmpty,Point(184,174))
  drawSlots(img,404,170,0,true)
  img:drawImage(buttonDisabled,Point(408,368))
  img:drawImage(buttonIdle,Point(596,368))
  return img
end

local selected=itemSelectedState(false)
local target=itemSelectedState(true)
local empty=itemEmptyState()
save(selected,previewDir.."/poker_item_select_stage_preview_960x540_0_3_7.png")
save(selected,outputDir.."/poker_item_select_stage_preview_960x540_0_3_7.png")
save(target,previewDir.."/poker_item_target_select_stage_preview_960x540_0_3_7.png")
save(target,outputDir.."/poker_item_target_select_stage_preview_960x540_0_3_7.png")
save(empty,previewDir.."/poker_item_empty_stage_preview_960x540_0_3_7.png")
save(empty,outputDir.."/poker_item_empty_stage_preview_960x540_0_3_7.png")

local itemStates=Image(1920,540,ColorMode.RGB); itemStates:clear(C.ink)
itemStates:drawImage(selected,Point(0,0)); itemStates:drawImage(target,Point(960,0))
fill(itemStates,957,0,6,540,C.brassHi)
save(itemStates,previewDir.."/poker_item_selection_states_1920x540_0_3_7.png")
save(itemStates,outputDir.."/poker_item_selection_states_1920x540_0_3_7.png")

local itemSource=Sprite(960,540,ColorMode.RGB)
itemSource.layers[1].name="empty_selected_target"
setImage(itemSource,empty); itemSource.frames[1].duration=0.25
local fs=itemSource:newEmptyFrame(); itemSource:newCel(itemSource.layers[1],fs,selected,Point(0,0)); fs.duration=0.25
local ft=itemSource:newEmptyFrame(); itemSource:newCel(itemSource.layers[1],ft,target,Point(0,0)); ft.duration=0.25
itemSource:saveAs(sourceDir.."/poker_item_selection_states_0_3_7.aseprite")
itemSource:close()

print("Halli + Poker item UI 0.3.7 generated")
