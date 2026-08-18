-- ItemExpansion 0.5.2.1 art review addendum.
-- Reuses approved 0.5.2 runtime sprites and generates QA previews only.

local p=app.params
local baseDir=assert(p.baseDir)
local runtimeDir=assert(p.runtimeDir)
local previewDir=assert(p.previewDir)
local sourceDir=assert(p.sourceDir)
local cardDir=assert(p.cardDir)
local T=Color{r=0,g=0,b=0,a=0}
local C={ink=Color{r=8,g=6,b=5,a=255},panel=Color{r=18,g=12,b=9,a=232},panel2=Color{r=34,g=20,b=13,a=238},brass0=Color{r=83,g=46,b=14,a=255},brass1=Color{r=160,g=93,b=25,a=255},brass2=Color{r=226,g=153,b=45,a=255},red=Color{r=222,g=55,b=48,a=255}}

local function image(w,h)local im=Image(w,h,ColorMode.RGB);im:clear(T);return im end
local function fill(im,x,y,w,h,c)
  x=math.floor(x);y=math.floor(y);w=math.floor(w);h=math.floor(h)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end end
end
local function frame(im,x,y,w,h,c,t)t=t or 1;fill(im,x,y,w,t,c);fill(im,x,y+h-t,w,t,c);fill(im,x,y,t,h,c);fill(im,x+w-t,y,t,h,c)end
local function line(im,x0,y0,x1,y1,c,t)
  t=t or 1;local dx=math.abs(x1-x0);local sx=x0<x1 and 1 or -1;local dy=-math.abs(y1-y0);local sy=y0<y1 and 1 or -1;local e=dx+dy
  while true do fill(im,x0-math.floor(t/2),y0-math.floor(t/2),t,t,c);if x0==x1 and y0==y1 then break end;local e2=2*e;if e2>=dy then e=e+dy;x0=x0+sx end;if e2<=dx then e=e+dx;y0=y0+sy end end
end
local function diamond(im,cx,cy,r,c)for y=-r,r do local s=r-math.abs(y);fill(im,cx-s,cy+y,s*2+1,1,c)end end
local function nearest(src,w,h)
  local d=image(w,h);for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h));for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w));d:drawPixel(x,y,src:getPixel(sx,sy))end end;return d
end
local function load(path)local s=assert(app.open(path),"cannot open "..path);local q=Image(s.cels[1].image);s:close();return q end
local function draw(dst,src,x,y,w,h)dst:drawImage(nearest(src,w,h),Point(x,y))end
local function copy(src)local d=image(src.width,src.height);d:drawImage(src,Point(0,0));return d end

local base=load(baseDir.."/halli_western_saloon_background.png")
local charge0=load(runtimeDir.."/prediction_insurance_charges_0_32_0_5_2.png")
local charge1=load(runtimeDir.."/prediction_insurance_charges_1_32_0_5_2.png")
local charge2=load(runtimeDir.."/prediction_insurance_charges_2_32_0_5_2.png")
local actual=load(runtimeDir.."/prediction_result_actual_success_32_0_5_2.png")
local insured=load(runtimeDir.."/prediction_result_insured_success_32_0_5_2.png")
local insuranceIcon=load(runtimeDir.."/item_prediction_insurance_popup_80_0_5_2.png")
local inkIcon=load(runtimeDir.."/item_wild_ink_popup_80_0_5_2.png")
local inkSeal=load(runtimeDir.."/wild_ink_suit_seal_4_32_0_5_2.png")
local inkApplied=load(runtimeDir.."/wild_ink_card_applied_marker_32_0_5_2.png")
local inkLocked=load(runtimeDir.."/wild_ink_exchange_locked_marker_32_0_5_2.png")
local barrelReady=load(runtimeDir.."/barrel_defense_ready_64_0_5_2.png")
local barrelBroken=load(runtimeDir.."/barrel_defense_broken_64_0_5_2.png")
local hpSaved=load(runtimeDir.."/barrel_hp_preserved_marker_32_0_5_2.png")
local mercTarget=load(runtimeDir.."/mercenary_player_target_marker_32_0_5_2.png")
local mercHidden=load(runtimeDir.."/mercenary_ai_hidden_marker_32_0_5_2.png")
local mercIcon=load(runtimeDir.."/item_mercenary_popup_80_0_5_2.png")
local cardA=load(cardDir.."/card_poker_spades_a.png")
local cardB=load(cardDir.."/card_poker_hearts_9.png")
local cardC=load(cardDir.."/card_poker_clubs_10.png")
local cardBack=load(cardDir.."/../card_back.png")

local function drawCard(dst,src,x,y,w,h)fill(dst,x+3,y+4,w,h,Color{r=0,g=0,b=0,a=150});draw(dst,src,x,y,w,h)end
local function drawResultPanel(dst,badge,badgeSize,charge,flash)
  fill(dst,310,188,340,156,C.panel);frame(dst,310,188,340,156,C.brass0,3);frame(dst,320,198,320,136,C.brass1,1)
  draw(dst,insuranceIcon,334,226,72,72)
  if flash then diamond(dst,488,262,47,Color{r=116,g=71,b=18,a=116});line(dst,438,262,538,262,C.brass2,2);line(dst,488,212,488,312,C.brass2,2)end
  draw(dst,badge,488-math.floor(badgeSize/2),262-math.floor(badgeSize/2),badgeSize,badgeSize);draw(dst,charge,582,250,32,32);draw(dst,actual,582,292,24,24);line(dst,580,292,608,320,C.red,3)
end

local frames={}
for i=1,4 do
  local stage=copy(base);fill(stage,0,0,960,540,Color{r=0,g=0,b=0,a=72})
  drawCard(stage,cardA,414,66,56,78);drawCard(stage,cardB,486,66,56,78);drawCard(stage,cardC,376,396,56,78);drawCard(stage,cardA,448,396,56,78);drawCard(stage,cardB,520,396,56,78)
  local sizes={24,48,64,48};local charges={charge2,charge2,charge1,charge1};drawResultPanel(stage,insured,sizes[i],charges[i],i==3);frames[i]=stage
end
local source=Sprite(960,540,ColorMode.RGB);source.layers[1].name="insurance_activation_review";source.cels[1].image:clear(T);source.cels[1].image:drawImage(frames[1],Point(0,0));source.frames[1].duration=.10
for i=2,4 do local f=source:newEmptyFrame();source:newCel(source.layers[1],f,frames[i],Point(0,0));f.duration=.10 end
source:saveAs(sourceDir.."/insurance_activation_review_4f_0_5_2_1.aseprite");source:close()
frames[4]:saveAs(previewDir.."/insurance_activation_review_960x540_0_5_2_1.png");nearest(frames[4],1280,720):saveAs(previewDir.."/insurance_activation_review_1280x720_0_5_2_1.png");nearest(frames[4],1920,1080):saveAs(previewDir.."/insurance_activation_review_1920x1080_0_5_2_1.png")

local timing=image(960,180);fill(timing,0,0,960,180,C.ink)
for i=1,4 do
  local x=10+(i-1)*238;fill(timing,x,10,226,160,C.panel2);frame(timing,x,10,226,160,C.brass0,2);draw(timing,insuranceIcon,x+18,50,64,64)
  local sizes={24,40,58,42};if i==3 then diamond(timing,x+132,82,42,Color{r=122,g=75,b=18,a=126})end;draw(timing,insured,x+132-math.floor(sizes[i]/2),82-math.floor(sizes[i]/2),sizes[i],sizes[i]);draw(timing,i<3 and charge2 or charge1,x+174,66,32,32);fill(timing,x+22,136,182,4,C.brass0);fill(timing,x+22,136,math.floor(182*(i/4)),4,C.brass2)
end
timing:saveAs(previewDir.."/insurance_activation_timing_4step_960x180_0_5_2_1.png")

local boundary=copy(base);fill(boundary,0,0,960,540,Color{r=0,g=0,b=0,a=88});local xs={34,270,506,742};for _,x in ipairs(xs)do fill(boundary,x,146,184,246,C.panel);frame(boundary,x,146,184,246,C.brass0,2)end
draw(boundary,inkIcon,88,166,80,80);drawCard(boundary,cardB,62,260,56,78);draw(boundary,inkSeal,82,301,28,28);draw(boundary,inkApplied,128,280,32,32);draw(boundary,inkLocked,163,280,32,32)
draw(boundary,barrelReady,294,172,64,64);draw(boundary,barrelBroken,294,270,64,64);draw(boundary,hpSaved,374,286,32,32)
draw(boundary,insuranceIcon,558,166,80,80);draw(boundary,charge2,532,282,32,32);draw(boundary,charge1,574,282,32,32);draw(boundary,charge0,616,282,32,32);draw(boundary,actual,554,334,32,32);draw(boundary,insured,606,334,32,32)
draw(boundary,mercIcon,794,166,80,80);drawCard(boundary,cardC,772,272,56,78);draw(boundary,mercTarget,784,294,32,32);for i=0,2 do drawCard(boundary,cardBack,838+i*4,268-i*3,56,78)end;draw(boundary,mercHidden,852,292,32,32)
boundary:saveAs(previewDir.."/item_boundary_review_960x540_0_5_2_1.png");nearest(boundary,1280,720):saveAs(previewDir.."/item_boundary_review_1280x720_0_5_2_1.png");nearest(boundary,1920,1080):saveAs(previewDir.."/item_boundary_review_1920x1080_0_5_2_1.png")
print("ItemExpansion 0.5.2.1 review previews generated")
