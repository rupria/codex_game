-- Poker prediction UI cleanup for issue #73.
-- Removes baked placeholder rules and keeps all labels as runtime-localized text.

local p = app.params
local outDir = assert(p.outDir)
local previewDir = assert(p.previewDir)
local sourceDir = assert(p.sourceDir)
local oldDir = assert(p.oldDir)
local backgroundPath = assert(p.background)
local cardRoot = assert(p.cardRoot)
local cardBackPath = assert(p.cardBack)
local cratePath = assert(p.crate)

local C = {
  clear=Color{r=0,g=0,b=0,a=0}, black=Color{r=5,g=5,b=5,a=255},
  panel=Color{r=22,g=15,b=11,a=255}, leather=Color{r=55,g=31,b=18,a=255},
  leatherHi=Color{r=83,g=47,b=25,a=255}, brassDark=Color{r=83,g=48,b=16,a=255},
  brass=Color{r=178,g=105,b=28,a=255}, brassHi=Color{r=244,g=187,b=70,a=255},
  teal=Color{r=34,g=203,b=205,a=255}, red=Color{r=226,g=65,b=67,a=255},
  cream=Color{r=239,g=221,b=181,a=255}
}

local function load(path)
  local sprite = assert(app.open(path), 'cannot open '..path)
  local image = Image(sprite.cels[1].image)
  sprite:close()
  return image
end

local function save(image, path)
  local sprite = Sprite(image.width, image.height, ColorMode.RGB)
  sprite.cels[1].image:clear(C.clear)
  sprite.cels[1].image:drawImage(image, Point(0, 0))
  sprite:saveAs(path)
  sprite:close()
end

local function image(width, height)
  local result = Image(width, height, ColorMode.RGB)
  result:clear(C.clear)
  return result
end

local function fill(target, x, y, width, height, color)
  for yy=math.max(0,y),math.min(target.height-1,y+height-1) do
    for xx=math.max(0,x),math.min(target.width-1,x+width-1) do
      target:drawPixel(xx, yy, color)
    end
  end
end

local function frame(target, x, y, width, height, color, thickness)
  thickness = thickness or 1
  fill(target,x,y,width,thickness,color)
  fill(target,x,y+height-thickness,width,thickness,color)
  fill(target,x,y,thickness,height,color)
  fill(target,x+width-thickness,y,thickness,height,color)
end

local function diamond(target, cx, cy, radius, color)
  for yy=-radius,radius do
    local span=radius-math.abs(yy)
    fill(target,cx-span,cy+yy,span*2+1,1,color)
  end
end

local function resize(source, width, height)
  local result=image(width,height)
  for y=0,height-1 do
    local sy=math.min(source.height-1,math.floor(y*source.height/height))
    for x=0,width-1 do
      local sx=math.min(source.width-1,math.floor(x*source.width/width))
      result:drawPixel(x,y,source:getPixel(sx,sy))
    end
  end
  return result
end

local function cleanPlaceholderRule(source, kind)
  local result=Image(source)
  local y0,y1,x0,x1 = 26,35,54,210
  if kind == 'continue' then y0,y1,x0,x1 = 17,27,28,136 end
  for y=y0,y1 do
    for x=x0,x1 do
      local pixel=result:getPixel(x,y)
      local color=Color(pixel)
      if color.alpha > 0 and color.red > 170 and color.green > 150 and color.blue > 115 then
        local sampleY=math.max(0,y-9)
        result:drawPixel(x,y,result:getPixel(x,sampleY))
      end
    end
  end
  return result
end

local function titlePlate()
  local result=image(320,48)
  fill(result,4,5,312,38,C.black)
  fill(result,7,8,306,32,C.brassDark)
  fill(result,10,11,300,26,C.leather)
  frame(result,12,13,296,22,C.leatherHi,1)
  diamond(result,18,24,4,C.brassHi)
  diamond(result,302,24,4,C.brass)
  return result
end

local states={'idle','hover','selected','disabled'}
local player,ai={},{}
for _,state in ipairs(states) do
  player[state]=cleanPlaceholderRule(load(oldDir..'/poker_prediction_player_'..state..'_232x64_0_5_7.png'),'prediction')
  ai[state]=cleanPlaceholderRule(load(oldDir..'/poker_prediction_ai_'..state..'_232x64_0_5_7.png'),'prediction')
  save(player[state],outDir..'/poker_prediction_player_'..state..'_232x64_0_6_0.png')
  save(ai[state],outDir..'/poker_prediction_ai_'..state..'_232x64_0_6_0.png')
end

local continueIdle=cleanPlaceholderRule(load(oldDir..'/poker_result_continue_idle_164x44_0_5_7.png'),'continue')
local continueHover=cleanPlaceholderRule(load(oldDir..'/poker_result_continue_hover_164x44_0_5_7.png'),'continue')
save(continueIdle,outDir..'/poker_result_continue_idle_164x44_0_6_0.png')
save(continueHover,outDir..'/poker_result_continue_hover_164x44_0_6_0.png')

local insurance=load(oldDir..'/poker_insurance_remaining_icon_28_0_5_7.png')
local success=load(oldDir..'/poker_prediction_success_icon_28_0_5_7.png')
local emblem=load(oldDir..'/poker_prediction_stage_emblem_40_0_5_7.png')
save(insurance,outDir..'/poker_insurance_remaining_icon_28_0_6_0.png')
save(success,outDir..'/poker_prediction_success_icon_28_0_6_0.png')
save(emblem,outDir..'/poker_prediction_stage_emblem_40_0_6_0.png')
local plate=titlePlate()
save(plate,outDir..'/poker_prediction_title_plate_320x48_0_6_0.png')

local controls=image(508,300)
fill(controls,0,0,508,300,Color{r=9,g=7,b=6,a=255})
for index,state in ipairs(states) do
  controls:drawImage(player[state],Point(8,8+(index-1)*72))
  controls:drawImage(ai[state],Point(260,8+(index-1)*72))
end
save(controls,sourceDir..'/poker_prediction_clean_controls_0_6_0.aseprite')
save(plate,sourceDir..'/poker_prediction_title_plate_0_6_0.aseprite')

local preview=load(backgroundPath)
assert(preview.width==960 and preview.height==540,'preview background must be 960x540')
local function card(path,x,y)
  local source=resize(load(path),56,78)
  fill(preview,x+4,y+5,56,78,Color{r=0,g=0,b=0,a=110})
  preview:drawImage(source,Point(x,y))
end

local back=resize(load(cardBackPath),56,78)
for index=0,2 do
  local x=384+index*68
  fill(preview,x+4,85,56,78,Color{r=0,g=0,b=0,a=110})
  preview:drawImage(back,Point(x,80))
end

card(cardRoot..'/card_poker_spades_j.png',416,218)
card(cardRoot..'/card_poker_clubs_6.png',488,218)
card(cardRoot..'/card_poker_clubs_j.png',380,338)
card(cardRoot..'/card_poker_hearts_7.png',452,338)
card(cardRoot..'/card_poker_spades_q.png',524,338)

preview:drawImage(plate,Point(320,20))
preview:drawImage(emblem,Point(332,24))
preview:drawImage(insurance,Point(690,112))
preview:drawImage(success,Point(624,428))
preview:drawImage(resize(load(cratePath),88,76),Point(650,350))
preview:drawImage(player.selected,Point(139,456))
preview:drawImage(ai.idle,Point(589,456))
preview:drawImage(continueIdle,Point(398,490))
save(preview,previewDir..'/poker_prediction_clean_round_table_preview_960x540_0_6_0.png')

local sheet=image(960,360)
fill(sheet,0,0,960,360,Color{r=9,g=7,b=6,a=255})
for index,state in ipairs(states) do
  sheet:drawImage(player[state],Point(20,20+(index-1)*80))
  sheet:drawImage(ai[state],Point(280,20+(index-1)*80))
end
sheet:drawImage(plate,Point(560,20))
sheet:drawImage(emblem,Point(572,24))
sheet:drawImage(insurance,Point(568,102))
sheet:drawImage(success,Point(568,174))
sheet:drawImage(continueIdle,Point(640,102))
sheet:drawImage(continueHover,Point(640,174))
save(sheet,previewDir..'/poker_prediction_clean_asset_states_960x360_0_6_0.png')

print('PokerPredictionClean 0.6.0 generated')
