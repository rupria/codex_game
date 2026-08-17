local p=app.params
local uiRoot=assert(p.uiRoot)
local newRoot=assert(p.newRoot)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local T=Color{r=0,g=0,b=0,a=0}
local C={bg=Color{r=8,g=6,b=5,a=255},panel=Color{r=23,g=16,b=11,a=255},old=Color{r=83,g=49,b=19,a=255},new=Color{r=35,g=216,b=218,a=255},gold=Color{r=236,g=178,b=61,a=255}}
local function image(w,h) local i=Image(w,h,ColorMode.RGB);i:clear(T);return i end
local function fill(im,x,y,w,h,c) for yy=y,y+h-1 do for xx=x,x+w-1 do if xx>=0 and yy>=0 and xx<im.width and yy<im.height then im:drawPixel(xx,yy,c) end end end end
local function frame(im,x,y,w,h,c,t) t=t or 1;fill(im,x,y,w,t,c);fill(im,x,y+h-t,w,t,c);fill(im,x,y,t,h,c);fill(im,x+w-t,y,t,h,c) end
local function load(path) local s=assert(app.open(path),"cannot open "..path);local i=Image(s.cels[1].image);s:close();return i end
local function nearest(src,w,h) local d=image(w,h);for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h));for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w));d:drawPixel(x,y,src:getPixel(sx,sy)) end end;return d end
local function cell(board,x,y,w,h,src,scale,accent)
  fill(board,x,y,w,h,C.panel);frame(board,x,y,w,h,accent,2);local q=nearest(src,math.floor(src.width*scale),math.floor(src.height*scale));board:drawImage(q,Point(x+math.floor((w-q.width)/2),y+math.floor((h-q.height)/2)))
end
local function old(rel) return load(uiRoot.."/"..rel) end
local function fresh(name) return load(newRoot.."/"..name) end
local oldItems={old("Gameplay_0_1_2/item_reload_64_0_1_2.png"),old("Gameplay_0_1_2/item_bottom_deal_64_0_1_2.png"),old("Gameplay_0_1_2/item_hype_man_64_0_1_2.png"),old("Gameplay_0_1_2/item_heal_tonic_64_0_1_2.png")}
local newItems={fresh("item_reload_western_64_0_5_0.png"),fresh("item_bottom_deal_western_64_0_5_0.png"),fresh("item_hype_man_western_64_0_5_0.png"),fresh("item_heal_tonic_western_64_0_5_0.png")}
local oldStatus={old("Halli_0_3_2/hp_heart_player_filled_24_0_3_2.png"),old("Halli_0_3_2/hp_heart_player_damage_24_0_3_2.png"),old("Halli_0_3_2/hp_heart_ai_filled_24_0_3_2.png"),old("Halli_0_3_2/hp_heart_ai_damage_24_0_3_2.png"),old("Halli_0_2_1/round_win_pip_player_filled_32_0_2_1.png"),old("Halli_0_2_1/round_win_pip_ai_filled_32_0_2_1.png")}
local newStatus={fresh("hp_heart_player_filled_24_0_5_0.png"),fresh("hp_heart_player_damage_24_0_5_0.png"),fresh("hp_heart_ai_filled_24_0_5_0.png"),fresh("hp_heart_ai_damage_24_0_5_0.png"),fresh("round_win_badge_player_filled_32_0_5_0.png"),fresh("round_win_badge_ai_filled_32_0_5_0.png")}
local oldUtility={old("Presentation_0_1_2_4/phase_three_call_icon_64_0_1_2_4.png"),old("Presentation_0_1_2_4/phase_showdown_icon_64_0_1_2_4.png"),old("Presentation_0_1_2_4/stage_item_limit_one_64_0_1_2_4.png"),old("Presentation_0_1_2_4/stage_item_limit_two_64_0_1_2_4.png"),old("Presentation_0_1_2_4/stage_item_card_restricted_64_0_1_2_4.png"),old("Presentation_0_1_2_4/stage_item_inventory_restricted_64_0_1_2_4.png")}
local newUtility={fresh("phase_three_call_western_64_0_5_0.png"),fresh("phase_showdown_western_64_0_5_0.png"),fresh("stage_item_limit_one_western_64_0_5_0.png"),fresh("stage_item_limit_two_western_64_0_5_0.png"),fresh("stage_item_card_restricted_western_64_0_5_0.png"),fresh("stage_item_inventory_restricted_western_64_0_5_0.png")}

local board=image(1280,720);fill(board,0,0,1280,720,C.bg);fill(board,638,0,4,720,C.gold)
-- header codes: one old bronze pip vs three new cyan diamonds, no text baked.
fill(board,48,34,540,6,C.old);fill(board,692,34,540,6,C.new)
for i=1,4 do cell(board,30+(i-1)*148,70,132,142,oldItems[i],1.55,C.old);cell(board,674+(i-1)*148,70,132,142,newItems[i],1.55,C.new) end
for i=1,6 do local scale=i<=4 and 2.3 or 1.8;cell(board,28+(i-1)*98,240,84,92,oldStatus[i],scale,C.old);cell(board,672+(i-1)*98,240,84,92,newStatus[i],scale,C.new) end
for i=1,6 do cell(board,24+((i-1)%3)*198,365+math.floor((i-1)/3)*158,180,142,oldUtility[i],1.55,C.old);cell(board,668+((i-1)%3)*198,365+math.floor((i-1)/3)*158,180,142,newUtility[i],1.55,C.new) end
board:saveAs(previewDir.."/icon_overhaul_before_after_1280x720_0_5_0.png")
board:saveAs(outputDir.."/icon_overhaul_before_after_1280x720_0_5_0.png")
print("Icon comparison generated")
