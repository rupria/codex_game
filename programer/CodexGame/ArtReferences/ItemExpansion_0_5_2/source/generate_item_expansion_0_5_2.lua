-- Codex Game item expansion 0.5.2
-- Aseprite-authored pixel assets for IT-04..IT-07.
-- No baked localization text, no opaque square icon backgrounds.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local popupBase=assert(p.popupBase)

local pc=app.pixelColor
local T=Color{r=0,g=0,b=0,a=0}
local C={
  ink=Color{r=9,g=7,b=7,a=255}, ink2=Color{r=25,g=18,b=15,a=255},
  leather0=Color{r=49,g=25,b=18,a=255}, leather1=Color{r=88,g=42,b=24,a=255},
  leather2=Color{r=143,g=72,b=37,a=255}, leather3=Color{r=202,g=112,b=55,a=255},
  wood0=Color{r=54,g=29,b=18,a=255}, wood1=Color{r=101,g=51,b=26,a=255},
  wood2=Color{r=157,g=83,b=38,a=255}, wood3=Color{r=207,g=125,b=54,a=255},
  brass0=Color{r=74,g=42,b=14,a=255}, brass1=Color{r=139,g=80,b=22,a=255},
  brass2=Color{r=210,g=139,b=39,a=255}, brass3=Color{r=255,g=203,b=78,a=255},
  brass4=Color{r=255,g=237,b=166,a=255},
  steel0=Color{r=33,g=37,b=39,a=255}, steel1=Color{r=67,g=74,b=77,a=255},
  steel2=Color{r=124,g=132,b=133,a=255}, steel3=Color{r=204,g=207,b=195,a=255},
  paper0=Color{r=130,g=102,b=64,a=255}, paper1=Color{r=204,g=173,b=122,a=255},
  paper2=Color{r=248,g=228,b=184,a=255}, bone=Color{r=240,g=219,b=175,a=255},
  red0=Color{r=85,g=20,b=22,a=255}, red1=Color{r=179,g=36,b=42,a=255}, red2=Color{r=246,g=68,b=66,a=255},
  cyan0=Color{r=5,g=65,b=74,a=255}, cyan1=Color{r=28,g=208,b=211,a=255}, cyan2=Color{r=139,g=255,b=240,a=255},
  green0=Color{r=25,g=52,b=34,a=255}, green1=Color{r=58,g=109,b=64,a=255},
  violet0=Color{r=48,g=24,b=62,a=255}, violet1=Color{r=103,g=46,b=118,a=255}, violet2=Color{r=176,g=85,b=179,a=255},
  orange=Color{r=238,g=104,b=24,a=255}, fire=Color{r=255,g=196,b=47,a=255},
  smoke=Color{r=83,g=75,b=69,a=220}, muted=Color{r=82,g=75,b=66,a=255}
}

local function image(w,h) local q=Image(w,h,ColorMode.RGB);q:clear(T);return q end
local function fill(im,x,y,w,h,c)
  x=math.floor(x);y=math.floor(y);w=math.floor(w);h=math.floor(h)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function hline(im,x0,x1,y,c,t) fill(im,x0,y,x1-x0+1,t or 1,c) end
local function frame(im,x,y,w,h,c,t) t=t or 1;fill(im,x,y,w,t,c);fill(im,x,y+h-t,w,t,c);fill(im,x,y,t,h,c);fill(im,x+w-t,y,t,h,c) end
local function disk(im,cx,cy,r,c) for y=-r,r do local s=math.floor(math.sqrt(math.max(0,r*r-y*y)));hline(im,cx-s,cx+s,cy+y,c) end end
local function ellipse(im,cx,cy,rx,ry,c) for y=-ry,ry do local s=math.floor(rx*math.sqrt(math.max(0,1-y*y/(ry*ry))));hline(im,cx-s,cx+s,cy+y,c) end end
local function diamond(im,cx,cy,r,c) for y=-r,r do local s=r-math.abs(y);hline(im,cx-s,cx+s,cy+y,c) end end
local function line(im,x0,y0,x1,y1,c,t)
  t=t or 1;local dx=math.abs(x1-x0);local sx=x0<x1 and 1 or -1;local dy=-math.abs(y1-y0);local sy=y0<y1 and 1 or -1;local e=dx+dy
  while true do fill(im,x0-math.floor(t/2),y0-math.floor(t/2),t,t,c);if x0==x1 and y0==y1 then break end;local e2=2*e;if e2>=dy then e=e+dy;x0=x0+sx end;if e2<=dx then e=e+dx;y0=y0+sy end end
end
local function poly(im,pts,c)
  local minY=9999;local maxY=-9999;for _,q in ipairs(pts) do minY=math.min(minY,q[2]);maxY=math.max(maxY,q[2]) end
  for y=minY,maxY do local xs={};local j=#pts;for i=1,#pts do local a=pts[i];local b=pts[j];if ((a[2]<y and b[2]>=y) or (b[2]<y and a[2]>=y)) then table.insert(xs,math.floor(a[1]+(y-a[2])/(b[2]-a[2])*(b[1]-a[1])+0.5)) end;j=i end;table.sort(xs);for i=1,#xs-1,2 do hline(im,xs[i],xs[i+1],y,c) end end
end
local function nearest(src,w,h)
  local d=image(w,h);for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h));for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w));d:drawPixel(x,y,src:getPixel(sx,sy)) end end;return d
end
local function copy(src) local d=image(src.width,src.height);d:drawImage(src,Point(0,0));return d end
local function outline(src,c,r)
  r=r or 1;local d=image(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do if pc.rgbaA(src:getPixel(x,y))==0 then local hit=false;for oy=-r,r do for ox=-r,r do local xx=x+ox;local yy=y+oy;if xx>=0 and yy>=0 and xx<src.width and yy<src.height and pc.rgbaA(src:getPixel(xx,yy))>24 then hit=true end end end;if hit then d:drawPixel(x,y,c) end end end end
  d:drawImage(src,Point(0,0));return d
end
local function shadowed(src)
  local d=image(src.width,src.height);for y=0,src.height-1 do for x=0,src.width-1 do if pc.rgbaA(src:getPixel(x,y))>24 and x+3<src.width and y+4<src.height then d:drawPixel(x+3,y+4,Color{r=0,g=0,b=0,a=145}) end end end;d:drawImage(outline(src,C.ink),Point(0,0));return d
end
local function glint(im,x,y,c) c=c or C.brass4;fill(im,x-1,y-5,3,11,c);fill(im,x-5,y-1,11,3,c);fill(im,x-2,y-2,5,5,c) end
local function smallSkull(im,x,y,s)
  ellipse(im,x,y,s,s-1,C.bone);fill(im,x-s+1,y,s*2-2,s,C.bone);disk(im,x-math.floor(s/2),y-1,math.max(1,math.floor(s/3)),C.ink);disk(im,x+math.floor(s/2),y-1,math.max(1,math.floor(s/3)),C.ink);fill(im,x-1,y+1,3,3,C.ink);for i=-2,2,2 do fill(im,x+i,y+s-1,1,3,C.ink) end
end
local function star(im,cx,cy,r,c)
  diamond(im,cx,cy,r,c);fill(im,cx-r-3,cy-1,r*2+7,3,c);fill(im,cx-1,cy-r-3,3,r*2+7,c);diamond(im,cx,cy,math.max(1,r-2),C.brass4)
end
local function heart(im,x,y,s,c) disk(im,x-s/2,y-s/3,s/2,c);disk(im,x+s/2,y-s/3,s/2,c);poly(im,{{x-s,y},{x+s,y},{x,y+s+2}},c) end
local function club(im,x,y,s,c) disk(im,x,y-s/2,s/2,c);disk(im,x-s/2,y,s/2,c);disk(im,x+s/2,y,s/2,c);poly(im,{{x-1,y},{x+2,y},{x+s/2,y+s}},c) end
local function spade(im,x,y,s,c) poly(im,{{x,y-s},{x-s,y+2},{x,y+s/2},{x+s,y+2}},c);fill(im,x-1,y,3,s+1,c) end
local function suit(im,id,x,y,s,c)
  if id==1 then spade(im,x,y,s,c) elseif id==2 then heart(im,x,y,s,c) elseif id==3 then club(im,x,y,s,c) else diamond(im,x,y,s,c) end
end
local function cardBack(im,x,y,w,h,accent)
  fill(im,x,y,w,h,C.ink2);frame(im,x,y,w,h,C.brass0,2);frame(im,x+3,y+3,w-6,h-6,accent,1);diamond(im,x+math.floor(w/2),y+math.floor(h/2),math.max(2,math.floor(w/6)),C.brass2);smallSkull(im,x+math.floor(w/2),y+math.floor(h/2),math.max(2,math.floor(w/8)))
end
local function save(im,name) im:saveAs(runtimeDir.."/"..name);im:saveAs(outputDir.."/"..name) end
local function saveSource(name,frames,duration)
  local s=Sprite(frames[1].width,frames[1].height,ColorMode.RGB);s.layers[1].name="authoritative_frames";s.cels[1].image:clear(T);s.cels[1].image:drawImage(frames[1],Point(0,0));for i=2,#frames do local f=s:newEmptyFrame();s:newCel(s.layers[1],f,frames[i],Point(0,0)) end;for _,f in ipairs(s.frames) do f.duration=duration or 0.1 end;s:saveAs(sourceDir.."/"..name);s:close()
end
local function sheet(frames)
  local d=image(frames[1].width*#frames,frames[1].height);for i,q in ipairs(frames) do d:drawImage(q,Point((i-1)*q.width,0)) end;return d
end
local function load(path) local s=assert(app.open(path),"cannot open "..path);local q=Image(s.cels[1].image);s:close();return q end

local function popupWildInk()
  local v=image(80,80)
  -- black-violet glass inkwell with brass foot and corked neck
  ellipse(v,39,63,26,8,C.brass0);ellipse(v,39,60,24,7,C.brass2);fill(v,21,36,37,26,C.violet0);poly(v,{{24,31},{54,31},{61,42},{57,61},{20,61},{17,42}},C.violet1);poly(v,{{25,34},{35,32},{31,58},{23,58},{20,43}},C.violet2);frame(v,22,35,34,25,C.brass1,2)
  fill(v,29,23,20,12,C.brass0);fill(v,31,24,16,10,C.ink2);frame(v,29,23,20,12,C.brass2,2);fill(v,32,19,14,5,C.leather1);frame(v,32,19,14,5,C.ink,1)
  star(v,39,47,7,C.brass2);smallSkull(v,39,47,4)
  -- ivory quill, deliberately offset so the bottle silhouette remains readable at 64px
  line(v,43,38,64,8,C.paper1,4);line(v,44,38,65,7,C.paper2,2);poly(v,{{63,8},{69,5},{67,17},{58,26}},C.paper2);line(v,61,15,67,8,C.paper0,1)
  -- four suit wax seals indicate effective-suit change without covering a card corner
  local pos={{13,24},{15,56},{64,31},{64,58}}
  for i,q in ipairs(pos) do disk(v,q[1],q[2],7,i%2==0 and C.red0 or C.brass0);disk(v,q[1],q[2],5,i%2==0 and C.red1 or C.brass2);suit(v,i,q[1],q[2],3,((i==2 or i==4) and C.paper2 or C.ink)) end
  glint(v,30,38,C.violet2);return shadowed(v)
end

local function popupBarrel()
  local v=image(80,80)
  -- stout whiskey barrel; broad shield silhouette differentiates it from other item vessels
  ellipse(v,39,20,25,10,C.wood0);ellipse(v,39,18,23,8,C.wood3);fill(v,14,19,50,41,C.wood1);poly(v,{{18,20},{28,22},{27,58},{17,59}},C.wood2);poly(v,{{42,20},{58,20},{62,58},{48,58}},C.wood2);ellipse(v,39,59,25,9,C.wood0);ellipse(v,39,57,23,7,C.wood2)
  fill(v,14,25,50,5,C.steel0);fill(v,16,26,46,2,C.steel2);fill(v,13,48,52,5,C.steel0);fill(v,16,49,46,2,C.steel2)
  line(v,30,22,29,56,C.wood0,2);line(v,48,21,49,56,C.wood0,2)
  -- sheriff shield plate communicates defense-ready state
  poly(v,{{39,28},{50,32},{48,47},{39,54},{30,47},{28,32}},C.brass0);poly(v,{{39,31},{47,34},{45,45},{39,50},{33,45},{31,34}},C.brass2);star(v,39,40,5,C.brass3)
  -- broken-plank notch is an identity cue for the later impact animation
  poly(v,{{58,16},{70,23},{66,29},{57,25}},C.wood0);line(v,60,19,67,24,C.wood3,2);glint(v,23,27,C.brass4);return shadowed(v)
end

local function popupInsurance()
  local v=image(80,80)
  -- folded policy scroll with leather corner guards
  poly(v,{{18,13},{59,10},{65,61},{22,67}},C.paper0);poly(v,{{21,16},{56,14},{61,58},{24,63}},C.paper2);frame(v,23,18,34,40,C.brass0,2)
  poly(v,{{17,13},{29,14},{24,25}},C.leather1);poly(v,{{59,10},{65,23},{54,19}},C.leather2);poly(v,{{22,67},{23,54},{34,62}},C.leather0)
  -- crossed score lines are symbolic only; no baked words
  line(v,30,25,51,23,C.paper0,2);line(v,29,30,48,28,C.paper0,1);line(v,29,35,42,34,C.paper0,1)
  star(v,42,46,9,C.brass1);smallSkull(v,42,46,5)
  -- two removable brass charges hang from the seal
  for i=0,1 do local x=58+i*8;fill(v,x,40,6,19,C.brass1);fill(v,x+1,40,4,17,C.brass3);poly(v,{{x,40},{x+1,35},{x+3,33},{x+5,36},{x+6,40}},C.leather3);fill(v,x,56,6,3,C.brass4) end
  line(v,53,38,61,31,C.red0,3);disk(v,55,37,4,C.red1);glint(v,46,41,C.brass4);return shadowed(v)
end

local function revolver(im,x,y,flip)
  local dir=flip and -1 or 1
  ellipse(im,x,y,8,7,C.steel0);ellipse(im,x,y,5,5,C.steel2);disk(im,x,y,2,C.brass2)
  if dir>0 then fill(im,x+5,y-3,23,6,C.steel0);fill(im,x+7,y-2,20,2,C.steel3);poly(im,{{x-3,y+5},{x+5,y+7},{x+1,y+23},{x-7,y+21}},C.leather2)
  else fill(im,x-28,y-3,23,6,C.steel0);fill(im,x-27,y-2,20,2,C.steel3);poly(im,{{x+3,y+5},{x-5,y+7},{x-1,y+23},{x+7,y+21}},C.leather2) end
end
local function cowboyHat(im,x,y,c)
  poly(im,{{x-14,y},{x-10,y-11},{x-5,y-15},{x,y-12},{x+6,y-15},{x+11,y-10},{x+14,y}},C.ink2);poly(im,{{x-10,y-1},{x-7,y-10},{x+7,y-10},{x+10,y-1}},c);fill(im,x-10,y-4,20,4,C.red0);poly(im,{{x-20,y},{x-9,y-2},{x+11,y-2},{x+20,y},{x+15,y+5},{x-15,y+5}},c)
end
local function popupMercenary()
  local v=image(80,80)
  -- two distinct hats over crossed revolvers: a squad, not a single-card replacement
  revolver(v,27,37,false);revolver(v,53,37,true)
  cowboyHat(v,29,22,C.leather2);cowboyHat(v,53,25,C.steel1)
  -- paired contract badges and hidden opponent card-back
  star(v,24,58,6,C.brass2);star(v,56,58,6,C.brass2)
  cardBack(v,33,47,14,21,C.red1);line(v,18,66,62,66,C.brass0,3);glint(v,40,35,C.brass4);return shadowed(v)
end

local ids={"wild_ink","barrel","prediction_insurance","mercenary"}
local masters={popupWildInk(),popupBarrel(),popupInsurance(),popupMercenary()}

local function desaturate(src)
  local d=image(src.width,src.height);for y=0,src.height-1 do for x=0,src.width-1 do local px=src:getPixel(x,y);local a=pc.rgbaA(px);if a>0 then local g=math.floor((pc.rgbaR(px)*3+pc.rgbaG(px)*5+pc.rgbaB(px)*2)/10);d:drawPixel(x,y,Color{r=math.floor(g*.72),g=math.floor(g*.68),b=math.floor(g*.62),a=a}) end end end;return d
end
local function stateIcon(master,state)
  local v=nearest(master,64,64)
  if state=="hover" then v=outline(v,C.brass4,1);glint(v,53,10,C.brass4);diamond(v,8,54,3,C.brass3)
  elseif state=="selected" then v=outline(v,C.cyan1,1);frame(v,2,2,60,60,C.cyan1,2);diamond(v,8,8,4,C.cyan2);line(v,48,54,53,59,C.cyan2,3);line(v,53,59,61,48,C.cyan2,3)
  elseif state=="disabled" then v=desaturate(v);line(v,10,12,54,54,C.steel0,6);line(v,11,12,55,54,C.steel2,2);disk(v,51,13,7,C.steel0);fill(v,48,10,7,7,C.ink);fill(v,49,7,5,7,C.steel2);frame(v,49,7,5,8,C.steel0,1)
  end
  return v
end

local stateNames={"default","hover","selected","disabled"}
local allStates={}
for i,id in ipairs(ids) do
  save(masters[i],"item_"..id.."_popup_80_0_5_2.png")
  for _,st in ipairs(stateNames) do local q=stateIcon(masters[i],st);table.insert(allStates,q);save(q,"item_"..id.."_"..st.."_64_0_5_2.png") end
end
saveSource("item_expansion_popup_masters_80_0_5_2.aseprite",masters,0.2)
saveSource("item_expansion_inventory_states_64_0_5_2.aseprite",allStates,0.2)

-- Wild Ink: ink spread frames are transparent card overlays; final seal sits away from rank/suit corners.
local inkFrames={}
for i=0,7 do local v=image(64,64);local r=3+i*3;ellipse(v,34,36,r,math.max(2,math.floor(r*.55)),i<4 and C.violet1 or C.violet0);if i>1 then for n=1,i do disk(v,16+n*6,44+(n%2)*4,1+(n%3==0 and 1 or 0),C.violet2) end end;if i>=5 then disk(v,48,49,9,C.brass0);disk(v,48,49,7,C.red1);suit(v,4,48,49,4,C.paper2) end;table.insert(inkFrames,v) end
save(sheet(inkFrames),"wild_ink_spread_8f_512x64_0_5_2.png");saveSource("wild_ink_spread_8f_0_5_2.aseprite",inkFrames,0.08125)
local suitSeals={};for i=1,4 do local v=image(32,32);disk(v,16,16,12,C.brass0);disk(v,16,16,9,(i==2 or i==4) and C.red1 or C.paper2);suit(v,i,16,16,6,(i==2 or i==4) and C.paper2 or C.ink);table.insert(suitSeals,v);save(v,"wild_ink_suit_seal_"..i.."_32_0_5_2.png") end
save(sheet(suitSeals),"wild_ink_suit_seals_4x32_128x32_0_5_2.png");saveSource("wild_ink_suit_seals_4x32_0_5_2.aseprite",suitSeals,0.2)
local inkMarker=image(32,32);disk(inkMarker,16,16,12,C.violet0);star(inkMarker,16,16,6,C.brass1);save(inkMarker,"wild_ink_card_applied_marker_32_0_5_2.png")
local exchangeLock=image(32,32);cardBack(exchangeLock,8,5,16,22,C.steel1);line(exchangeLock,6,7,26,26,C.red2,4);save(exchangeLock,"wild_ink_exchange_locked_marker_32_0_5_2.png")

-- Barrel: ready -> impact spark -> cracked and broken. Impact uses shape and debris, not color alone.
local barrelFrames={}
for i=0,7 do local v=nearest(masters[2],64,64);if i>=2 and i<=4 then local x=49+(i-2)*3;glint(v,x,30,C.fire);for n=1,i do line(v,x,30,x+(n%2==0 and 10 or -7),30-n*5,C.orange,2) end end;if i>=4 then line(v,22,19,42,48,C.ink,3);line(v,42,48,55,28,C.ink,3);for n=1,i-3 do local x=9+n*8;poly(v,{{x,52},{x+5,48},{x+8,55},{x+2,59}},C.wood2) end end;if i>=6 then fill(v,24,26,31,23,T);line(v,17,55,49,22,C.wood3,6);line(v,19,56,52,24,C.ink,2) end;table.insert(barrelFrames,v) end
save(sheet(barrelFrames),"barrel_defense_impact_break_8f_512x64_0_5_2.png");saveSource("barrel_defense_impact_break_8f_0_5_2.aseprite",barrelFrames,0.06875)
save(barrelFrames[1],"barrel_defense_ready_64_0_5_2.png");save(barrelFrames[8],"barrel_defense_broken_64_0_5_2.png")
local hpKeep=image(32,32);poly(hpKeep,{{16,3},{27,8},{25,22},{16,29},{7,22},{5,8}},C.brass1);heart(hpKeep,16,15,6,C.red2);save(hpKeep,"barrel_hp_preserved_marker_32_0_5_2.png")

-- Prediction Insurance: seal application and charge/result badges.
local insuranceFrames={}
for i=0,5 do local v=image(64,64);local s=3+i*2;disk(v,32,32,s,C.red0);if i>1 then star(v,32,32,math.min(9,2+i),C.brass2) end;if i>=4 then smallSkull(v,32,32,5) end;if i==5 then glint(v,47,17,C.brass4) end;table.insert(insuranceFrames,v) end
save(sheet(insuranceFrames),"prediction_insurance_apply_6f_384x64_0_5_2.png");saveSource("prediction_insurance_apply_6f_0_5_2.aseprite",insuranceFrames,0.075)
for charges=0,2 do local v=image(32,32);poly(v,{{16,2},{28,8},{26,24},{16,30},{6,24},{4,8}},charges==0 and C.steel0 or C.brass0);for n=1,2 do local x=11+(n-1)*10;if n<=charges then fill(v,x,10,5,13,C.brass2);fill(v,x+1,10,3,11,C.brass4) else frame(v,x,10,5,13,C.steel2,1) end end;if charges==0 then line(v,7,7,25,25,C.red2,3) end;save(v,"prediction_insurance_charges_"..charges.."_32_0_5_2.png") end
local actual=image(32,32);star(actual,16,16,9,C.brass2);fill(actual,11,15,4,8,C.cyan1);fill(actual,16,11,4,12,C.cyan2);save(actual,"prediction_result_actual_success_32_0_5_2.png")
local insured=image(32,32);poly(insured,{{16,2},{29,9},{26,24},{16,30},{6,24},{3,9}},C.brass0);star(insured,16,15,7,C.brass2);fill(insured,12,14,3,7,C.paper2);fill(insured,16,10,3,11,C.paper2);save(insured,"prediction_result_insured_success_32_0_5_2.png")

-- Mercenary: two-lane simultaneous exchange. AI card backs never turn face-up.
local mercFrames={}
for i=0,9 do local v=image(96,96);local t=i/9;local px=8+math.floor(30*t);local ax=70-math.floor(30*t);local y1=18+math.floor(36*t);local y2=54-math.floor(36*t);cardBack(v,px,y1,18,28,C.cyan1);cardBack(v,ax,y2,18,28,C.red1);line(v,10,79,84,79,C.brass0,2);if i>1 then line(v,18,70,42,49,C.cyan1,2);line(v,78,26,54,47,C.red1,2) end;if i==4 or i==5 then glint(v,48,48,C.brass4) end;table.insert(mercFrames,v) end
save(sheet(mercFrames),"mercenary_simultaneous_exchange_10f_960x96_0_5_2.png");saveSource("mercenary_simultaneous_exchange_10f_0_5_2.aseprite",mercFrames,0.09)
local playerTarget=image(32,32);frame(playerTarget,4,3,24,26,C.cyan1,2);diamond(playerTarget,16,16,5,C.cyan2);save(playerTarget,"mercenary_player_target_marker_32_0_5_2.png")
local aiHidden=image(32,32);cardBack(aiHidden,7,3,18,26,C.red1);fill(aiHidden,13,12,6,8,C.ink);fill(aiHidden,14,9,4,5,C.steel2);save(aiHidden,"mercenary_ai_hidden_marker_32_0_5_2.png")

-- Contact sheet: popup masters, all four state shapes and effect strips.
local contact=image(1280,720);fill(contact,0,0,1280,720,Color{r=8,g=6,b=5,a=255})
for i=1,4 do local x=20+(i-1)*315;fill(contact,x,20,295,315,C.ink2);frame(contact,x,20,295,315,C.brass0,3);contact:drawImage(nearest(masters[i],144,144),Point(x+75,34));for s=1,4 do contact:drawImage(allStates[(i-1)*4+s],Point(x+16+(s-1)*68,205)) end end
fill(contact,20,360,1240,330,C.ink2);frame(contact,20,360,1240,330,C.brass0,3)
contact:drawImage(nearest(sheet(inkFrames),512,64),Point(42,390));contact:drawImage(nearest(sheet(barrelFrames),512,64),Point(42,486));contact:drawImage(nearest(sheet(insuranceFrames),384,64),Point(42,582));contact:drawImage(nearest(sheet(mercFrames),640,64),Point(600,390));for i=1,4 do contact:drawImage(suitSeals[i],Point(640+(i-1)*50,486)) end;contact:drawImage(actual,Point(900,486));contact:drawImage(insured,Point(950,486));for c=0,2 do local q=load(runtimeDir.."/prediction_insurance_charges_"..c.."_32_0_5_2.png");contact:drawImage(q,Point(1050+c*45,486)) end
contact:saveAs(previewDir.."/item_expansion_full_contact_sheet_1280x720_0_5_2.png");contact:saveAs(outputDir.."/item_expansion_full_contact_sheet_1280x720_0_5_2.png")

-- Application mockup uses the already connected Poker item popup composition.
local stage=load(popupBase)
for i=1,4 do stage:drawImage(allStates[(i-1)*4+1],Point(408+(i-1)*80,158)) end
stage:drawImage(masters[3],Point(416,252));stage:drawImage(load(runtimeDir.."/prediction_insurance_charges_2_32_0_5_2.png"),Point(506,280));stage:drawImage(actual,Point(549,280));stage:drawImage(insured,Point(587,280))
stage:saveAs(previewDir.."/item_expansion_application_preview_960x540_0_5_2.png");stage:saveAs(outputDir.."/item_expansion_application_preview_960x540_0_5_2.png")
nearest(stage,1280,720):saveAs(previewDir.."/item_expansion_safearea_preview_1280x720_0_5_2.png")
nearest(stage,1920,1080):saveAs(previewDir.."/item_expansion_safearea_preview_1920x1080_0_5_2.png")

-- Card-safe overlay proof: markers remain in center/right safe zones, never over rank/suit corners.
local safe=image(960,540);fill(safe,0,0,960,540,Color{r=12,g=18,b=15,a=255});ellipse(safe,480,280,430,220,Color{r=20,g=48,b=39,a=255})
for i=0,2 do local x=255+i*105;fill(safe,x,155,84,126,C.paper2);frame(safe,x,155,84,126,C.ink,3);fill(safe,x+8,164,12,18,C.ink);suit(safe,(i%4)+1,x+70,170,7,(i==1) and C.red1 or C.ink);safe:drawImage(suitSeals[i+1],Point(x+47,235)) end
safe:drawImage(barrelFrames[1],Point(520,185));safe:drawImage(insured,Point(602,205));safe:drawImage(mercFrames[6],Point(650,162))
safe:saveAs(previewDir.."/item_expansion_card_safezone_preview_960x540_0_5_2.png");safe:saveAs(outputDir.."/item_expansion_card_safezone_preview_960x540_0_5_2.png")

print("Item expansion 0.5.2 generated")
