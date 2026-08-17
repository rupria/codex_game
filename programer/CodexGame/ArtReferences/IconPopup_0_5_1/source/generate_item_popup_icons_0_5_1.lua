-- High-detail item icons for the 80x80 Poker item popup.
-- Aseprite source is authoritative; 64px inventory icons are separate exports.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local popupBase=assert(p.popupBase)

local pc=app.pixelColor
local T=Color{r=0,g=0,b=0,a=0}
local C={
  ink=Color{r=10,g=8,b=6,a=255}, ink2=Color{r=26,g=17,b=11,a=255},
  leather0=Color{r=54,g=27,b=17,a=255}, leather1=Color{r=91,g=43,b=23,a=255},
  leather2=Color{r=142,g=70,b=34,a=255}, leather3=Color{r=197,g=107,b=51,a=255},
  brass0=Color{r=76,g=43,b=16,a=255}, brass1=Color{r=145,g=82,b=23,a=255},
  brass2=Color{r=211,g=137,b=39,a=255}, brass3=Color{r=255,g=202,b=79,a=255},
  brass4=Color{r=255,g=235,b=157,a=255}, steel0=Color{r=37,g=41,b=42,a=255},
  steel1=Color{r=72,g=78,b=79,a=255}, steel2=Color{r=123,g=130,b=128,a=255},
  steel3=Color{r=194,g=199,b=186,a=255}, bone=Color{r=241,g=220,b=174,a=255},
  paper=Color{r=248,g=230,b=191,a=255}, paper2=Color{r=207,g=177,b=129,a=255},
  red0=Color{r=91,g=21,b=24,a=255}, red1=Color{r=188,g=37,b=43,a=255},
  red2=Color{r=246,g=70,b=67,a=255}, cyan0=Color{r=7,g=72,b=80,a=255},
  cyan1=Color{r=35,g=210,b=210,a=255}, cyan2=Color{r=136,g=255,b=239,a=255},
  amber0=Color{r=101,g=38,b=12,a=255}, amber1=Color{r=173,g=71,b=18,a=255},
  amber2=Color{r=241,g=131,b=38,a=255}, green0=Color{r=31,g=62,b=39,a=255},
  green1=Color{r=77,g=123,b=67,a=255}, smoke=Color{r=95,g=83,b=72,a=210}
}

local function image(w,h) local q=Image(w,h,ColorMode.RGB);q:clear(T);return q end
local function fill(im,x,y,w,h,c)
  x=math.floor(x);y=math.floor(y);w=math.floor(w);h=math.floor(h)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end end
end
local function hline(im,x0,x1,y,c,t) fill(im,x0,y,x1-x0+1,t or 1,c) end
local function frame(im,x,y,w,h,c,t) t=t or 1;fill(im,x,y,w,t,c);fill(im,x,y+h-t,w,t,c);fill(im,x,y,t,h,c);fill(im,x+w-t,y,t,h,c) end
local function disk(im,cx,cy,r,c) for y=-r,r do local s=math.floor(math.sqrt(r*r-y*y));hline(im,cx-s,cx+s,cy+y,c) end end
local function ellipse(im,cx,cy,rx,ry,c) for y=-ry,ry do local s=math.floor(rx*math.sqrt(math.max(0,1-y*y/(ry*ry))));hline(im,cx-s,cx+s,cy+y,c) end end
local function diamond(im,cx,cy,r,c) for y=-r,r do local s=r-math.abs(y);hline(im,cx-s,cx+s,cy+y,c) end end
local function line(im,x0,y0,x1,y1,c,t)
  t=t or 1;local dx=math.abs(x1-x0);local sx=x0<x1 and 1 or -1;local dy=-math.abs(y1-y0);local sy=y0<y1 and 1 or -1;local e=dx+dy
  while true do fill(im,x0-math.floor(t/2),y0-math.floor(t/2),t,t,c);if x0==x1 and y0==y1 then break end;local e2=2*e;if e2>=dy then e=e+dy;x0=x0+sx end;if e2<=dx then e=e+dx;y0=y0+sy end end
end
local function poly(im,pts,c)
  local minY=999;local maxY=-999;for _,q in ipairs(pts) do minY=math.min(minY,q[2]);maxY=math.max(maxY,q[2]) end
  for y=minY,maxY do local xs={};local j=#pts;for i=1,#pts do local a=pts[i];local b=pts[j];if ((a[2]<y and b[2]>=y) or (b[2]<y and a[2]>=y)) then table.insert(xs,math.floor(a[1]+(y-a[2])/(b[2]-a[2])*(b[1]-a[1])+0.5)) end;j=i end;table.sort(xs);for i=1,#xs-1,2 do hline(im,xs[i],xs[i+1],y,c) end end
end
local function outline(src,c)
  local d=image(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do if pc.rgbaA(src:getPixel(x,y))==0 then local hit=false;for oy=-1,1 do for ox=-1,1 do local xx=x+ox;local yy=y+oy;if xx>=0 and yy>=0 and xx<src.width and yy<src.height and pc.rgbaA(src:getPixel(xx,yy))>24 then hit=true end end end;if hit then d:drawPixel(x,y,c) end end end end
  d:drawImage(src,Point(0,0));return d
end
local function shadowed(src)
  local d=image(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do if pc.rgbaA(src:getPixel(x,y))>24 and x+3<src.width and y+4<src.height then d:drawPixel(x+3,y+4,Color{r=0,g=0,b=0,a=145}) end end end
  d:drawImage(outline(src,C.ink),Point(0,0));return d
end
local function nearest(src,w,h)
  local d=image(w,h);for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h));for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w));d:drawPixel(x,y,src:getPixel(sx,sy)) end end;return d
end
local function glint(im,x,y,c) c=c or C.brass4;fill(im,x-1,y-5,3,11,c);fill(im,x-5,y-1,11,3,c);fill(im,x-2,y-2,5,5,c) end
local function smallSkull(im,x,y,s)
  ellipse(im,x,y,s,s-1,C.bone);fill(im,x-s+1,y,s*2-2,s,C.bone);disk(im,x-math.floor(s/2),y-1,math.max(1,math.floor(s/3)),C.ink);disk(im,x+math.floor(s/2),y-1,math.max(1,math.floor(s/3)),C.ink);fill(im,x-1,y+1,3,3,C.ink);for i=-2,2,2 do fill(im,x+i,y+s-1,1,3,C.ink) end
end
local function save(im,name) im:saveAs(runtimeDir.."/"..name);im:saveAs(outputDir.."/"..name) end
local function saveSource(name,frames)
  local s=Sprite(frames[1].width,frames[1].height,ColorMode.RGB);s.layers[1].name="popup_item_masters";s.cels[1].image:clear(T);s.cels[1].image:drawImage(frames[1],Point(0,0));for i=2,#frames do local f=s:newEmptyFrame();s:newCel(s.layers[1],f,frames[i],Point(0,0)) end;for _,f in ipairs(s.frames) do f.duration=0.2 end;s:saveAs(sourceDir.."/"..name);s:close()
end
local function load(path) local s=assert(app.open(path),"cannot open "..path);local q=Image(s.cels[1].image);s:close();return q end

local function popupReload()
  local v=image(80,80)
  -- heavy steel cylinder with six chambers and engraved skull boss
  ellipse(v,35,45,27,20,C.steel0);ellipse(v,35,40,27,20,C.steel1);ellipse(v,35,36,24,16,C.steel3);ellipse(v,35,39,21,15,C.steel1)
  local holes={{35,25},{48,31},{50,44},{35,51},{21,44},{22,31}}
  for _,q in ipairs(holes) do disk(v,q[1],q[2],5,C.ink);disk(v,q[1],q[2],3,C.steel0);fill(v,q[1]-1,q[2]-2,2,2,C.steel2) end
  disk(v,35,39,8,C.brass0);disk(v,35,39,6,C.brass2);smallSkull(v,35,39,4)
  line(v,11,57,57,61,C.steel0,5);line(v,14,55,54,58,C.steel2,2)
  -- ejector rod and loose cartridges
  line(v,12,21,21,54,C.steel2,4);line(v,13,20,20,53,C.steel3,1)
  local function bullet(x,y,tilt)
    if tilt then poly(v,{{x,y+6},{x+6,y+3},{x+15,y+25},{x+9,y+28}},C.brass1);poly(v,{{x,y+6},{x+1,y+1},{x+4,y-1},{x+7,y+3},{x+6,y+5}},C.leather3);line(v,x+7,y+7,x+13,y+23,C.brass3,2)
    else fill(v,x,y+8,8,23,C.brass1);fill(v,x+2,y+8,5,22,C.brass2);fill(v,x+4,y+10,2,15,C.brass4);poly(v,{{x,y+8},{x+2,y+2},{x+4,y},{x+7,y+3},{x+8,y+8}},C.leather3);fill(v,x,y+29,8,3,C.brass3) end
  end
  bullet(60,15,false);bullet(57,40,true);glint(v,22,23,C.brass4);return shadowed(v)
end

local function popupBottomDeal()
  local v=image(80,80)
  -- three-card deck with stitched leather back
  poly(v,{{12,14},{54,9},{60,51},{18,57}},C.paper2);line(v,17,18,53,13,C.ink,3);line(v,19,54,57,48,C.ink,2)
  poly(v,{{16,10},{58,7},{63,47},{21,52}},C.paper);line(v,21,15,53,12,C.brass0,2);line(v,24,48,58,44,C.brass0,2)
  diamond(v,30,28,7,C.red1);diamond(v,30,28,4,C.red2);smallSkull(v,46,29,5)
  -- clearly exposed bottom card
  poly(v,{{13,49},{58,44},{66,59},{21,67}},C.paper);line(v,18,53,58,49,C.red0,3);diamond(v,55,55,5,C.red1);fill(v,24,59,23,2,C.paper2)
  -- articulated leather glove pulling the bottom edge
  poly(v,{{41,61},{48,47},{57,46},{62,51},{72,53},{72,63},{62,68},{51,73}},C.leather0)
  poly(v,{{45,62},{51,51},{57,51},{56,57},{68,55},{69,61},{58,66},{51,69}},C.leather3)
  line(v,49,62,61,60,C.brass4,1);line(v,54,52,67,57,C.leather1,2);return shadowed(v)
end

local function popupHype()
  local v=image(80,80)
  -- Modern handheld megaphone: familiar red/ivory body with believable replaceable brass hardware.
  -- Draw the grip first so the silhouette remains a manufactured megaphone rather than a speaking horn.
  poly(v,{{27,39},{40,41},{36,65},{29,70},{22,66}},C.steel0)
  poly(v,{{29,43},{37,44},{33,62},{28,65},{25,63}},C.steel2)
  fill(v,25,53,10,11,C.red0);line(v,27,55,34,55,C.red2,2);line(v,26,61,33,61,C.ink,2)
  fill(v,28,42,10,4,C.brass1);line(v,29,43,36,43,C.brass3,1)
  fill(v,8,28,18,17,C.red0);fill(v,10,30,14,13,C.red1);fill(v,11,32,5,9,C.red2);frame(v,8,28,18,17,C.ink,2)
  frame(v,10,30,14,13,C.brass1,1);fill(v,6,32,4,9,C.brass0);fill(v,7,33,3,7,C.brass3)

  -- Ivory plastic cone stays familiar; the lip, seam, pivot and fasteners become polished brass.
  poly(v,{{22,26},{59,13},{69,14},{72,21},{72,45},{68,51},{59,51},{22,40}},C.paper2)
  poly(v,{{24,29},{59,18},{65,20},{66,44},{59,47},{24,38}},C.paper)
  line(v,27,31,58,22,C.brass3,2);line(v,27,37,58,44,C.brass1,2)
  ellipse(v,67,32,10,22,C.brass0);ellipse(v,67,32,8,19,C.brass2);ellipse(v,68,32,5,14,C.ink2)
  line(v,62,16,62,48,C.brass3,2);fill(v,64,18,2,8,C.brass4);fill(v,64,38,2,7,C.brass1)
  disk(v,22,34,5,C.brass0);disk(v,22,34,3,C.brass3);disk(v,22,34,1,C.ink)

  -- Cowboy hat sits on top of the megaphone body, not behind it.
  poly(v,{{32,16},{35,8},{39,4},{45,6},{50,4},{56,8},{59,16}},C.leather0)
  poly(v,{{36,14},{39,8},{45,8},{49,7},{54,9},{56,15}},C.leather2)
  fill(v,36,12,21,4,C.red0);fill(v,40,12,14,2,C.red2);disk(v,53,14,2,C.brass2)
  -- Wide upturned brim is deliberately oversized so it reads as cowboy, not fedora, at 56px.
  poly(v,{{22,16},{30,15},{36,14},{58,15},{65,17},{71,15},{68,21},{59,23},{32,22},{24,20}},C.leather0)
  poly(v,{{27,17},{37,16},{58,17},{66,18},{63,20},{58,21},{33,20}},C.leather3)

  -- Compact sound marks keep the silhouette readable at 56px inventory size.
  line(v,75,19,79,16,C.brass3,2);line(v,76,32,79,32,C.brass3,2);line(v,75,46,79,49,C.brass3,2)
  return shadowed(v)
end

local function popupHeal()
  local v=image(80,80)
  -- cork, wax seal and amber glass shoulders
  fill(v,29,5,22,10,C.leather0);frame(v,29,5,22,10,C.ink,2);fill(v,33,3,14,4,C.paper2);line(v,31,10,49,10,C.red0,2)
  poly(v,{{25,16},{55,16},{62,25},{61,67},{19,67},{18,25}},C.amber0)
  poly(v,{{22,22},{56,20},{57,63},{23,64}},C.amber1);poly(v,{{26,22},{34,20},{33,62},{26,63}},C.amber2);fill(v,29,24,3,31,C.brass4);fill(v,55,25,3,34,C.leather0)
  frame(v,18,23,44,45,C.ink,2);line(v,21,28,59,27,C.brass2,2);line(v,22,62,58,61,C.brass1,2)
  -- paper label with bone medicine mark and skull-bell seal
  poly(v,{{24,33},{55,31},{56,56},{23,58}},C.paper2);poly(v,{{27,34},{52,33},{53,54},{26,56}},C.paper)
  disk(v,40,44,9,C.red0);fill(v,37,36,7,17,C.bone);fill(v,32,41,17,7,C.bone);smallSkull(v,40,44,4)
  -- tied herbs and small leather strap
  line(v,58,48,71,59,C.green0,3);line(v,61,50,73,47,C.green1,2);line(v,63,53,74,55,C.green1,2);line(v,58,47,62,64,C.leather2,2)
  glint(v,29,25,C.brass4);return shadowed(v)
end

local popup={popupReload(),popupBottomDeal(),popupHype(),popupHeal()}
local ids={"reload","bottom_deal","hype_man","heal_tonic"}
for i=1,4 do
  save(popup[i],"item_"..ids[i].."_popup_80_0_5_1.png")
  save(nearest(popup[i],64,64),"item_"..ids[i].."_inventory_64_0_5_1.png")
end
saveSource("item_popup_icons_high_detail_80_0_5_1.aseprite",popup)

-- Detail sheet: exact 80px top, 2x inspection bottom.
local detail=image(960,360);fill(detail,0,0,960,360,Color{r=8,g=6,b=5,a=255})
for i=1,4 do local x=25+(i-1)*235;fill(detail,x,20,210,130,C.ink2);frame(detail,x,20,210,130,C.brass0,3);detail:drawImage(popup[i],Point(x+65,43));fill(detail,x,174,210,166,C.ink2);frame(detail,x,174,210,166,C.brass1,3);detail:drawImage(nearest(popup[i],144,144),Point(x+33,184)) end
detail:saveAs(previewDir.."/item_popup_icons_detail_contact_sheet_960x360_0_5_1.png");detail:saveAs(outputDir.."/item_popup_icons_detail_contact_sheet_960x360_0_5_1.png")

-- Application preview matches current code coordinates: 56px slot icons and an 80px detail icon.
local stage=load(popupBase)
for i=1,4 do stage:drawImage(nearest(popup[i],56,56),Point(412+(i-1)*80,162)) end
stage:drawImage(popup[4],Point(416,252))
stage:saveAs(previewDir.."/item_popup_icons_application_preview_960x540_0_5_1.png");stage:saveAs(outputDir.."/item_popup_icons_application_preview_960x540_0_5_1.png")

-- Hype Man revision proof: exact popup size, inventory draw size and enlarged pixel inspection.
local hypeProof=image(640,320);fill(hypeProof,0,0,640,320,Color{r=8,g=6,b=5,a=255})
fill(hypeProof,20,20,180,280,C.ink2);frame(hypeProof,20,20,180,280,C.brass0,3)
hypeProof:drawImage(popup[3],Point(70,64));hypeProof:drawImage(nearest(popup[3],56,56),Point(82,190))
fill(hypeProof,220,20,400,280,C.ink2);frame(hypeProof,220,20,400,280,C.brass1,3)
hypeProof:drawImage(nearest(popup[3],240,240),Point(300,40))
hypeProof:saveAs(previewDir.."/item_hype_man_modern_megaphone_preview_640x320_0_5_1.png");hypeProof:saveAs(outputDir.."/item_hype_man_modern_megaphone_preview_640x320_0_5_1.png")

local hypeStage=load(popupBase)
for i=1,4 do hypeStage:drawImage(nearest(popup[i],56,56),Point(412+(i-1)*80,162)) end
hypeStage:drawImage(popup[3],Point(416,252))
hypeStage:saveAs(previewDir.."/item_hype_man_popup_application_preview_960x540_0_5_1.png");hypeStage:saveAs(outputDir.."/item_hype_man_popup_application_preview_960x540_0_5_1.png")

print("Item popup icon set 0.5.1 generated")
