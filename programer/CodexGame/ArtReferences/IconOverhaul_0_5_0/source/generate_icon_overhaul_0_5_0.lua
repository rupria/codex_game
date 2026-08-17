-- Codex Game Western icon overhaul 0.5.0.
-- Generated and authored in Aseprite. Runtime icons keep transparent backgrounds;
-- contact sheets are presentation-only.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)

local pc=app.pixelColor
local T=Color{r=0,g=0,b=0,a=0}
local C={
  ink=Color{r=12,g=9,b=7,a=255}, ink2=Color{r=27,g=18,b=12,a=255},
  shadow=Color{r=42,g=24,b=14,a=255}, leatherDark=Color{r=70,g=34,b=19,a=255},
  leather=Color{r=119,g=58,b=29,a=255}, leatherHi=Color{r=174,g=91,b=42,a=255},
  brassDark=Color{r=83,g=49,b=19,a=255}, brass=Color{r=183,g=112,b=34,a=255},
  brassHi=Color{r=247,g=190,b=69,a=255}, brassWhite=Color{r=255,g=230,b=151,a=255},
  steelDark=Color{r=48,g=52,b=54,a=255}, steel=Color{r=98,g=103,b=103,a=255},
  steelHi=Color{r=174,g=178,b=166,a=255}, bone=Color{r=239,g=217,b=170,a=255},
  paper=Color{r=247,g=229,b=190,a=255}, paperShade=Color{r=202,g=174,b=127,a=255},
  cyanDark=Color{r=5,g=67,b=76,a=255}, cyan=Color{r=35,g=216,b=218,a=255},
  cyanHi=Color{r=132,g=255,b=244,a=255}, redDark=Color{r=92,g=22,b=25,a=255},
  red=Color{r=238,g=55,b=60,a=255}, redHi=Color{r=255,g=130,b=108,a=255},
  greenDark=Color{r=29,g=64,b=42,a=255}, green=Color{r=64,g=127,b=76,a=255},
  greenHi=Color{r=135,g=190,b=119,a=255}, amber=Color{r=151,g=65,b=18,a=255},
  amberHi=Color{r=235,g=122,b=36,a=255}, smoke=Color{r=90,g=78,b=67,a=230},
  disabled=Color{r=73,g=69,b=64,a=255}, black=Color{r=5,g=5,b=5,a=255}
}

local function img(w,h)
  local v=Image(w,h,ColorMode.RGB); v:clear(T); return v
end
local function fill(im,x,y,w,h,c)
  x=math.floor(x);y=math.floor(y);w=math.floor(w);h=math.floor(h)
  local x0=math.max(0,x); local y0=math.max(0,y)
  local x1=math.min(im.width-1,x+w-1); local y1=math.min(im.height-1,y+h-1)
  if x1<x0 or y1<y0 then return end
  for yy=y0,y1 do for xx=x0,x1 do im:drawPixel(xx,yy,c) end end
end
local function hline(im,x0,x1,y,c,t) fill(im,x0,y,x1-x0+1,t or 1,c) end
local function vline(im,x,y0,y1,c,t) fill(im,x,y0,t or 1,y1-y0+1,c) end
local function frame(im,x,y,w,h,c,t)
  t=t or 1; fill(im,x,y,w,t,c); fill(im,x,y+h-t,w,t,c)
  fill(im,x,y,t,h,c); fill(im,x+w-t,y,t,h,c)
end
local function disk(im,cx,cy,r,c)
  for yy=-r,r do local s=math.floor(math.sqrt(r*r-yy*yy)); hline(im,cx-s,cx+s,cy+yy,c) end
end
local function ellipse(im,cx,cy,rx,ry,c)
  for yy=-ry,ry do local q=1-(yy*yy)/(ry*ry); local s=math.floor(rx*math.sqrt(math.max(0,q))); hline(im,cx-s,cx+s,cy+yy,c) end
end
local function diamond(im,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(im,cx-s,cx+s,cy+yy,c) end
end
local function line(im,x0,y0,x1,y1,c,t)
  t=t or 1; local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1; local e=dx+dy
  while true do fill(im,x0-math.floor(t/2),y0-math.floor(t/2),t,t,c); if x0==x1 and y0==y1 then break end
    local e2=2*e; if e2>=dy then e=e+dy; x0=x0+sx end; if e2<=dx then e=e+dx; y0=y0+sy end
  end
end
local function poly(im,pts,c)
  local minY=9999; local maxY=-9999
  for _,q in ipairs(pts) do minY=math.min(minY,q[2]); maxY=math.max(maxY,q[2]) end
  for y=minY,maxY do
    local xs={}; local j=#pts
    for i=1,#pts do local a=pts[i]; local b=pts[j]
      if ((a[2]<y and b[2]>=y) or (b[2]<y and a[2]>=y)) then
        table.insert(xs,math.floor(a[1]+(y-a[2])/(b[2]-a[2])*(b[1]-a[1])+0.5))
      end; j=i
    end
    table.sort(xs); for i=1,#xs-1,2 do hline(im,xs[i],xs[i+1],y,c) end
  end
end
local function outline(src,c)
  local o=img(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do
    if pc.rgbaA(src:getPixel(x,y))==0 then
      local hit=false
      for oy=-1,1 do for ox=-1,1 do local xx=x+ox;local yy=y+oy
        if xx>=0 and yy>=0 and xx<src.width and yy<src.height and pc.rgbaA(src:getPixel(xx,yy))>24 then hit=true end
      end end
      if hit then o:drawPixel(x,y,c) end
    end
  end end
  o:drawImage(src,Point(0,0)); return o
end
local function shadowed(src)
  local o=img(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do
    if pc.rgbaA(src:getPixel(x,y))>24 and x+2<src.width and y+3<src.height then o:drawPixel(x+2,y+3,Color{r=0,g=0,b=0,a=135}) end
  end end
  o:drawImage(outline(src,C.ink),Point(0,0)); return o
end
local function nearest(src,w,h)
  local d=img(w,h)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); d:drawPixel(x,y,src:getPixel(sx,sy)) end
  end; return d
end
local function tint(src,c,mul)
  mul=mul or 0.55; local d=img(src.width,src.height)
  for y=0,src.height-1 do for x=0,src.width-1 do local q=src:getPixel(x,y); local a=pc.rgbaA(q)
    if a>0 then d:drawPixel(x,y,pc.rgba(math.floor(pc.rgbaR(q)*(1-mul)+c.red*mul),math.floor(pc.rgbaG(q)*(1-mul)+c.green*mul),math.floor(pc.rgbaB(q)*(1-mul)+c.blue*mul),a)) end
  end end; return d
end
local function save(im,name)
  im:saveAs(runtimeDir.."/"..name); im:saveAs(outputDir.."/"..name)
end
local function saveSource(name,frames,layerName,durations)
  local s=Sprite(frames[1].width,frames[1].height,ColorMode.RGB); s.layers[1].name=layerName
  s.cels[1].image:clear(T); s.cels[1].image:drawImage(frames[1],Point(0,0))
  for i=2,#frames do local f=s:newEmptyFrame(); s:newCel(s.layers[1],f,frames[i],Point(0,0)) end
  for i,f in ipairs(s.frames) do f.duration=durations and durations[i] or 0.18 end
  s:saveAs(sourceDir.."/"..name); s:close()
end
local function glint(im,x,y,c)
  c=c or C.brassWhite; fill(im,x-1,y-4,3,9,c); fill(im,x-4,y-1,9,3,c); fill(im,x-2,y-2,5,5,c)
end

-- Functional item icons: detailed standalone props, no baked circular plaques.
local function iconReload()
  local v=img(64,64)
  ellipse(v,29,35,20,15,C.steelDark); ellipse(v,29,32,20,15,C.steel); ellipse(v,29,29,18,12,C.steelHi)
  ellipse(v,29,31,15,10,C.steelDark); disk(v,29,31,5,C.brassDark)
  local holes={{29,22},{39,27},{36,38},{22,39},{18,27}}
  for _,q in ipairs(holes) do disk(v,q[1],q[2],4,C.ink); disk(v,q[1],q[2],2,C.shadow) end
  fill(v,11,42,36,5,C.steelDark); fill(v,14,46,30,3,C.ink)
  local function bullet(x,y)
    fill(v,x,y+7,6,14,C.brassDark); fill(v,x+1,y+7,4,13,C.brass); fill(v,x+2,y+8,2,9,C.brassHi)
    poly(v,{{x,y+7},{x+1,y+2},{x+3,y},{x+5,y+2},{x+6,y+7}},C.leatherHi); fill(v,x+2,y+2,2,5,C.brassWhite)
  end
  bullet(45,12); bullet(51,23); line(v,46,48,55,54,C.leather,4); line(v,47,48,55,52,C.leatherHi,1)
  glint(v,17,19,C.brassWhite); return shadowed(v)
end
local function iconBottomDeal()
  local v=img(64,64)
  -- worn deck
  poly(v,{{11,15},{42,11},{46,39},{14,43}},C.paperShade); frame(v,13,15,31,28,C.ink,2)
  poly(v,{{14,12},{45,9},{48,37},{17,40}},C.paper); line(v,18,17,39,15,C.brassDark,1); diamond(v,22,23,4,C.red)
  -- exposed bottom card
  poly(v,{{13,38},{47,34},{50,46},{17,52}},C.paper); line(v,17,42,45,39,C.redDark,2); diamond(v,42,43,3,C.red)
  -- leather glove pinching card
  poly(v,{{34,46},{41,37},{48,37},{51,41},{58,43},{57,51},{48,54},{42,58}},C.leatherDark)
  poly(v,{{38,47},{43,40},{48,41},{47,46},{54,45},{55,49},{47,52},{42,55}},C.leatherHi)
  line(v,40,49,49,48,C.brassWhite,1); line(v,17,55,42,58,C.shadow,3); return shadowed(v)
end
local function iconHype()
  local v=img(64,64)
  -- battered promoter hat behind the horn
  fill(v,9,12,28,5,C.ink); poly(v,{{14,12},{18,5},{34,6},{37,13}},C.leatherDark); fill(v,18,10,17,3,C.redDark)
  -- brass speaking horn
  poly(v,{{15,36},{31,29},{53,14},{59,20},{38,39},{29,46}},C.brassDark)
  poly(v,{{19,35},{32,31},{52,18},{55,21},{36,36},{29,41}},C.brass)
  poly(v,{{51,14},{61,13},{61,25},{55,22}},C.brassHi); line(v,56,16,58,22,C.brassWhite,2)
  fill(v,11,32,11,10,C.steelDark); fill(v,13,34,8,6,C.steelHi)
  -- red bandana and sound rays
  poly(v,{{20,43},{31,41},{38,51},{28,55}},C.redDark); diamond(v,33,48,5,C.red)
  line(v,51,8,58,4,C.brassHi,2); line(v,55,29,62,32,C.brassHi,2); line(v,45,11,48,4,C.brassHi,2)
  return shadowed(v)
end
local function iconHeal()
  local v=img(64,64)
  fill(v,24,7,17,8,C.leatherDark); frame(v,24,7,17,8,C.ink,2); fill(v,27,5,11,4,C.paperShade)
  poly(v,{{21,16},{44,16},{48,23},{47,53},{18,53},{17,23}},C.amber)
  fill(v,21,19,22,31,C.amberHi); fill(v,24,20,5,27,Color{r=255,g=175,b=64,a=255}); fill(v,42,22,3,25,C.leatherDark)
  frame(v,17,20,31,34,C.ink,2); fill(v,20,29,25,17,C.paper); frame(v,20,29,25,17,C.paperShade,1)
  disk(v,32,37,7,C.redDark); fill(v,30,32,5,11,C.bone); fill(v,27,35,11,5,C.bone)
  line(v,19,24,45,24,C.brassHi,2); glint(v,26,22,C.brassWhite); return shadowed(v)
end

local items={iconReload(),iconBottomDeal(),iconHype(),iconHeal()}
local itemNames={
  "item_reload_western_64_0_5_0.png","item_bottom_deal_western_64_0_5_0.png",
  "item_hype_man_western_64_0_5_0.png","item_heal_tonic_western_64_0_5_0.png"
}
for i=1,#items do save(items[i],itemNames[i]) end
saveSource("gameplay_item_icons_western_0_5_0.aseprite",items,"IT01_IT02_IT03_HP01")

-- Currency and warning icons.
local function bulletIcon(size,temporary)
  local v=img(size,size); local cx=math.floor(size/2); local scale=size/40
  local x=math.floor(cx-7*scale); local y=math.floor(4*scale); local w=math.max(5,math.floor(14*scale))
  fill(v,x,y+13*scale,w,20*scale,C.brassDark); fill(v,x+2*scale,y+13*scale,w-4*scale,19*scale,C.brass)
  fill(v,x+4*scale,y+14*scale,3*scale,15*scale,C.brassHi)
  poly(v,{{x,y+13*scale},{x+2*scale,y+5*scale},{cx,y},{x+w-2*scale,y+5*scale},{x+w,y+13*scale}},temporary and C.steel or C.leatherHi)
  fill(v,x,y+31*scale,w,4*scale,C.brassHi); fill(v,x+2*scale,y+34*scale,w-4*scale,2*scale,C.ink)
  if temporary then
    line(v,cx-4*scale,y+9*scale,cx+3*scale,y+17*scale,C.ink,math.max(1,math.floor(scale*2)))
    line(v,cx+3*scale,y+17*scale,cx-2*scale,y+22*scale,C.ink,math.max(1,math.floor(scale*2)))
    -- small hourglass tag
    local hx=math.floor(size*0.68); local hy=math.floor(size*0.62); local hs=math.max(1,math.floor(scale))
    frame(v,hx,hy,math.floor(9*scale),math.floor(11*scale),C.brassHi,hs)
    line(v,hx+2*scale,hy+2*scale,hx+7*scale,hy+8*scale,C.bone,hs); line(v,hx+7*scale,hy+2*scale,hx+2*scale,hy+8*scale,C.bone,hs)
  else glint(v,math.floor(cx-3*scale),math.floor(y+17*scale),C.brassWhite) end
  return shadowed(v)
end
local currencyBasic=bulletIcon(40,false); local currencyTemp=bulletIcon(40,true)
local priceBullet=nearest(currencyBasic,24,24)
local warning=img(24,24); diamond(warning,12,12,11,C.brassDark); diamond(warning,12,12,8,C.brassHi); diamond(warning,12,12,5,C.ink)
fill(warning,11,6,3,8,C.bone); fill(warning,11,16,3,3,C.red); warning=outline(warning,C.ink)
save(currencyBasic,"currency_basic_bullet_western_40_0_5_0.png")
save(currencyTemp,"currency_temporary_cracked_round_40_0_5_0.png")
save(priceBullet,"shop_price_bullet_western_24_0_5_0.png")
save(warning,"shop_exit_warning_badge_western_24_0_5_0.png")

local expireFrames={}; local expireSheet=img(320,40)
for f=1,8 do
  local q=img(40,40); local cutoff=math.floor((f-1)*5)
  for y=0,39 do for x=0,39 do local pix=currencyTemp:getPixel(x,y); local a=pc.rgbaA(pix)
    if a>0 and y<40-cutoff then q:drawPixel(x,y,pix) end
  end end
  for s=1,f+1 do local x=(9+s*7+f*3)%36+2; local y=35-((s*5+f*2)%16); diamond(q,x,y,math.max(1,3-math.floor(f/4)),s%2==0 and C.brassHi or C.smoke) end
  expireFrames[f]=q; expireSheet:drawImage(q,Point((f-1)*40,0))
end
save(expireSheet,"currency_temporary_expire_western_8f_320x40_0_5_0.png")
saveSource("currency_temporary_expire_western_8f_0_5_0.aseprite",expireFrames,"crack_fall_and_fade",{0.07,0.07,0.07,0.07,0.08,0.08,0.09,0.11})
local warningFrames={}; local warningSheet=img(144,24)
for f=1,6 do local q=img(24,24); q:drawImage(warning,Point(0,0)); if f==2 or f==3 or f==5 then glint(q,17,6,C.brassWhite) end
  warningFrames[f]=q; warningSheet:drawImage(q,Point((f-1)*24,0)) end
save(warningSheet,"shop_exit_warning_pulse_western_6f_144x24_0_5_0.png")
saveSource("shop_exit_warning_pulse_western_6f_0_5_0.aseprite",warningFrames,"warning_pulse",{0.08,0.08,0.08,0.08,0.08,0.12})

-- HP hearts and outcome pips.
local function heartIcon(color,hi,state)
  local v=img(24,24); local dark=(color==C.cyan) and C.cyanDark or C.redDark
  poly(v,{{3,8},{5,4},{9,3},{12,6},{15,3},{19,4},{21,8},{20,12},{12,21},{4,12}},state=="empty" and C.ink2 or dark)
  if state~="empty" then poly(v,{{5,8},{7,5},{10,5},{12,8},{15,5},{18,6},{19,9},{12,18},{6,11}},color); fill(v,7,6,4,3,hi) end
  if state=="empty" then line(v,5,8,12,18,C.disabled,2); line(v,12,18,19,8,C.disabled,2) end
  if state=="damage" then line(v,13,5,10,10,C.bone,2); line(v,10,10,14,13,C.ink,2); line(v,14,13,10,18,C.bone,2) end
  v=outline(v,C.ink); fill(v,2,10,2,3,C.brassDark); fill(v,20,10,2,3,C.brassDark); return v
end
local hearts={
  heartIcon(C.cyan,C.cyanHi,"filled"),heartIcon(C.cyan,C.cyanHi,"damage"),heartIcon(C.cyan,C.cyanHi,"empty"),
  heartIcon(C.red,C.redHi,"filled"),heartIcon(C.red,C.redHi,"damage"),heartIcon(C.red,C.redHi,"empty")
}
local heartNames={"hp_heart_player_filled_24_0_5_0.png","hp_heart_player_damage_24_0_5_0.png","hp_heart_player_empty_24_0_5_0.png","hp_heart_ai_filled_24_0_5_0.png","hp_heart_ai_damage_24_0_5_0.png","hp_heart_ai_empty_24_0_5_0.png"}
for i=1,#hearts do save(hearts[i],heartNames[i]) end
saveSource("hp_hearts_western_0_5_0.aseprite",hearts,"player_and_ai_hp")

local function sheriffPip(color,filledState)
  local v=img(32,32); local dark=(color==C.cyan) and C.cyanDark or C.redDark
  -- six-point sheriff badge
  poly(v,{{16,2},{20,9},{28,7},{24,14},{30,19},{21,21},{20,29},{15,23},{9,29},{9,21},{2,19},{8,14},{4,7},{12,9}},filledState and dark or C.ink2)
  if filledState then disk(v,16,16,8,color); disk(v,16,16,4,C.brassHi); disk(v,16,16,2,C.brassWhite) else disk(v,16,16,8,C.shadow); frame(v,12,12,9,9,C.disabled,1) end
  return outline(v,filledState and C.brassDark or C.ink)
end
local roundPips={sheriffPip(C.cyan,false),sheriffPip(C.cyan,true),sheriffPip(C.red,false),sheriffPip(C.red,true)}
local roundNames={"round_win_badge_player_empty_32_0_5_0.png","round_win_badge_player_filled_32_0_5_0.png","round_win_badge_ai_empty_32_0_5_0.png","round_win_badge_ai_filled_32_0_5_0.png"}
for i=1,4 do save(roundPips[i],roundNames[i]) end
local predEmpty=nearest(sheriffPip(C.brass,false),28,28); local predFilled=nearest(sheriffPip(C.brassHi,true),28,28)
save(predEmpty,"prediction_success_badge_empty_28_0_5_0.png"); save(predFilled,"prediction_success_badge_filled_28_0_5_0.png")

-- Portrait selectors: engraved cowboy vs outlaw, not flat silhouettes.
local function portraitBase(team,selected,hover)
  local v=img(88,88); local accent=team=="player" and C.cyan or C.red; local dark=team=="player" and C.cyanDark or C.redDark
  fill(v,5,5,78,78,C.ink); frame(v,2,2,84,84,C.brassDark,3); frame(v,7,7,74,74,selected and accent or (hover and C.brassHi or C.leatherDark),2)
  diamond(v,44,4,4,selected and accent or C.brass); diamond(v,44,83,4,selected and accent or C.brass)
  -- coat and shoulders
  poly(v,{{17,75},{22,57},{33,50},{55,50},{66,57},{72,75}},dark); line(v,17,75,72,75,C.brassDark,3)
  fill(v,29,52,30,20,C.leatherDark); poly(v,{{29,52},{38,60},{44,54},{51,60},{59,52},{57,72},{31,72}},C.steelDark)
  -- face and hat
  fill(v,32,27,24,25,C.bone); fill(v,35,24,18,4,C.leatherDark); fill(v,20,24,49,7,C.ink)
  poly(v,{{28,24},{32,14},{56,15},{61,25}},team=="player" and C.leatherDark or C.ink2)
  fill(v,33,20,24,4,accent); fill(v,35,35,5,4,C.ink); fill(v,49,35,5,4,C.ink)
  line(v,43,39,41,46,C.shadow,2); line(v,39,47,51,47,C.ink,2)
  if team=="ai" then fill(v,31,30,4,18,C.smoke); line(v,52,32,59,39,C.red,2); fill(v,48,37,6,3,C.redHi) end
  -- bandana and badge
  poly(v,{{34,52},{44,58},{55,52},{50,63},{39,63}},team=="player" and C.cyan or C.red)
  disk(v,60,59,5,C.brass); disk(v,60,59,2,C.brassWhite)
  if selected then glint(v,75,14,accent) elseif hover then glint(v,12,14,C.brassWhite) end
  return v
end
local portraits={}; local portraitNames={}
for _,team in ipairs({"player","ai"}) do for _,state in ipairs({"idle","hover","selected"}) do
  table.insert(portraits,portraitBase(team,state=="selected",state=="hover")); table.insert(portraitNames,"poker_predict_"..team.."_portrait_"..state.."_88_0_5_0.png")
end end
for i=1,#portraits do save(portraits[i],portraitNames[i]) end
saveSource("poker_prediction_portraits_western_0_5_0.aseprite",portraits,"player_ai_idle_hover_selected")

-- Community lock states.
local function lockIcon(state)
  local v=img(48,48); local metal=state=="reveal" and C.brassHi or C.steel
  -- horseshoe shackle
  for r=13,9,-1 do for a=195,345,3 do local rad=math.rad(a); local x=24+math.floor(math.cos(rad)*r); local y=20+math.floor(math.sin(rad)*r); fill(v,x,y,2,2,r>10 and C.ink or metal) end end
  fill(v,9,19,30,24,C.ink); fill(v,12,21,24,19,state=="locked" and C.steelDark or C.brassDark); frame(v,12,21,24,19,metal,2)
  disk(v,24,29,4,C.ink); poly(v,{{22,31},{26,31},{28,38},{20,38}},C.ink)
  if state=="reveal" then glint(v,36,17,C.brassWhite) end
  if state=="open" then line(v,31,18,40,11,C.brassHi,3); line(v,38,11,43,15,C.brassHi,2) end
  return shadowed(v)
end
local locks={lockIcon("locked"),lockIcon("reveal"),lockIcon("open")}
local lockNames={"community_lock_locked_48_0_5_0.png","community_lock_reveal_48_0_5_0.png","community_lock_open_48_0_5_0.png"}
for i=1,3 do save(locks[i],lockNames[i]) end
saveSource("community_lock_states_0_5_0.aseprite",locks,"locked_reveal_open")

-- AI thinking: revolver cylinder with a rotating live chamber.
local thinking={}; local thinkingSheet=img(384,48)
for f=1,8 do local v=img(48,48); disk(v,24,24,19,C.ink); disk(v,24,24,16,C.steelDark); disk(v,24,24,13,C.steel)
  for i=0,5 do local a=math.rad(i*60-90); local x=24+math.floor(math.cos(a)*9); local y=24+math.floor(math.sin(a)*9); disk(v,x,y,3,C.ink) end
  local a=math.rad((f-1)*45-90); local x=24+math.floor(math.cos(a)*14); local y=24+math.floor(math.sin(a)*14); disk(v,x,y,4,C.brassHi); disk(v,x,y,2,C.brassWhite)
  disk(v,24,24,4,C.brassDark); disk(v,24,24,2,C.brassHi); if f==2 or f==6 then glint(v,x,y,C.brassWhite) end
  v=outline(v,C.ink); thinking[f]=v; thinkingSheet:drawImage(v,Point((f-1)*48,0)) end
save(thinkingSheet,"ai_thinking_cylinder_western_8f_384x48_0_5_0.png")
saveSource("ai_thinking_cylinder_western_8f_0_5_0.aseprite",thinking,"rotating_live_chamber",{0.08,0.08,0.08,0.08,0.08,0.08,0.08,0.10})

-- Phase icons.
local function phaseThreeCall()
  local v=img(64,64)
  for i=0,2 do local x=11+i*18; fill(v,x+4,22,11,15,C.brassDark); ellipse(v,x+9,22,8,5,C.brass); fill(v,x+1,36,17,4,C.brassHi); disk(v,x+9,41,3,C.ink); fill(v,x+7,12,5,8,C.brassHi) end
  line(v,8,49,56,49,C.leatherHi,3); diamond(v,32,53,5,C.brassHi); return shadowed(v)
end
local function phaseShowdown()
  local v=img(64,64)
  poly(v,{{8,15},{33,10},{40,45},{15,49}},C.paperShade); frame(v,12,15,25,35,C.ink,2); diamond(v,22,26,5,C.red)
  poly(v,{{31,10},{56,15},{49,50},{25,45}},C.paper); frame(v,29,14,25,35,C.ink,2); diamond(v,44,27,5,C.ink)
  disk(v,32,45,10,C.brassDark); poly(v,{{32,33},{35,42},{45,42},{38,48},{41,58},{32,52},{23,58},{26,48},{19,42},{29,42}},C.brassHi); disk(v,32,46,4,C.red)
  return shadowed(v)
end
local phases={phaseThreeCall(),phaseShowdown()}
save(phases[1],"phase_three_call_western_64_0_5_0.png"); save(phases[2],"phase_showdown_western_64_0_5_0.png")

-- Item limit/restriction badges.
local function itemCaseBadge(kind)
  local v=img(64,64); fill(v,9,15,46,38,C.ink); fill(v,12,18,40,32,C.leatherDark); frame(v,12,18,40,32,C.brassDark,2)
  fill(v,21,11,22,8,C.leather); frame(v,21,11,22,8,C.brassDark,2); fill(v,26,15,12,4,C.ink)
  if kind=="one" or kind=="two" or kind=="used" or kind=="exhausted" then
    local count=(kind=="two") and 2 or 1
    for i=1,count do local x=kind=="two" and (19+(i-1)*17) or 27; fill(v,x,25,9,18,kind=="used" or kind=="exhausted" and C.disabled or C.brass); poly(v,{{x,25},{x+2,21},{x+7,21},{x+9,25}},kind=="used" or kind=="exhausted" and C.disabled or C.leatherHi) end
    if kind=="used" then line(v,18,24,47,46,C.red,4) elseif kind=="exhausted" then line(v,17,23,48,47,C.red,4); line(v,48,23,17,47,C.red,4) end
  elseif kind=="card_lock" then
    fill(v,21,22,22,25,C.paper); frame(v,21,22,22,25,C.ink,2); v:drawImage(nearest(locks[1],24,24),Point(31,31))
  elseif kind=="bag_lock" then
    poly(v,{{18,28},{23,20},{41,20},{46,28},{49,48},{15,48}},C.leather); line(v,21,28,43,28,C.brassHi,2); v:drawImage(nearest(locks[1],24,24),Point(31,31))
  end
  return shadowed(v)
end
local restrictions={itemCaseBadge("one"),itemCaseBadge("two"),itemCaseBadge("used"),itemCaseBadge("exhausted"),itemCaseBadge("card_lock"),itemCaseBadge("bag_lock")}
local restrictionNames={"stage_item_limit_one_western_64_0_5_0.png","stage_item_limit_two_western_64_0_5_0.png","stage_item_limit_used_one_western_64_0_5_0.png","stage_item_limit_exhausted_western_64_0_5_0.png","stage_item_card_restricted_western_64_0_5_0.png","stage_item_inventory_restricted_western_64_0_5_0.png"}
for i=1,#restrictions do save(restrictions[i],restrictionNames[i]) end
saveSource("stage_item_restriction_badges_western_0_5_0.aseprite",restrictions,"one_two_used_exhausted_card_inventory")

-- Guide/navigation glyphs. Single arrows only; never use >>.
local function navIcon(kind)
  local v=img(32,32); disk(v,16,16,14,C.ink2); disk(v,16,16,12,C.leatherDark); frame(v,5,5,22,22,C.brassDark,1)
  if kind=="prev" then poly(v,{{7,16},{17,7},{17,12},{25,12},{25,20},{17,20},{17,25}},C.cyan); line(v,9,16,17,9,C.cyanHi,1)
  elseif kind=="next" then poly(v,{{25,16},{15,7},{15,12},{7,12},{7,20},{15,20},{15,25}},C.red); line(v,23,16,15,9,C.redHi,1)
  else line(v,9,9,23,23,C.red,4); line(v,23,9,9,23,C.red,4); glint(v,24,8,C.brassWhite) end
  return outline(v,C.ink)
end
local nav={navIcon("prev"),navIcon("next"),navIcon("close")}
save(nav[1],"guide_nav_previous_western_32_0_5_0.png"); save(nav[2],"guide_nav_next_western_32_0_5_0.png"); save(nav[3],"guide_nav_close_western_32_0_5_0.png")

-- Presentation contact sheets.
local function cell(board,x,y,w,h,content,scale,accent)
  fill(board,x,y,w,h,C.ink2); frame(board,x,y,w,h,C.brassDark,3); frame(board,x+5,y+5,w-10,h-10,accent or C.leatherDark,1)
  local q=scale and nearest(content,math.floor(content.width*scale),math.floor(content.height*scale)) or content
  board:drawImage(q,Point(x+math.floor((w-q.width)/2),y+math.floor((h-q.height)/2)))
end

local itemSheet=img(640,180); fill(itemSheet,0,0,640,180,Color{r=11,g=8,b=6,a=255})
for i=1,4 do cell(itemSheet,20+(i-1)*155,20,135,140,items[i],1.65,i==1 and C.cyanDark or C.brassDark) end
itemSheet:saveAs(previewDir.."/icon_overhaul_items_contact_sheet_640x180_0_5_0.png"); itemSheet:saveAs(outputDir.."/icon_overhaul_items_contact_sheet_640x180_0_5_0.png")

local statusSheet=img(960,260); fill(statusSheet,0,0,960,260,Color{r=11,g=8,b=6,a=255})
for i=1,6 do cell(statusSheet,16+(i-1)*75,18,64,64,hearts[i],2,i<=3 and C.cyanDark or C.redDark) end
for i=1,4 do cell(statusSheet,490+(i-1)*108,18,94,94,roundPips[i],2,i<=2 and C.cyanDark or C.redDark) end
for i=1,4 do cell(statusSheet,22+(i-1)*118,126,104,112,({currencyBasic,currencyTemp,priceBullet,warning})[i],2,C.brassDark) end
for i=1,6 do cell(statusSheet,508+(i-1)*72,142,64,88,portraits[i],0.65,i<=3 and C.cyanDark or C.redDark) end
statusSheet:saveAs(previewDir.."/icon_overhaul_status_contact_sheet_960x260_0_5_0.png"); statusSheet:saveAs(outputDir.."/icon_overhaul_status_contact_sheet_960x260_0_5_0.png")

local utilitySheet=img(960,300); fill(utilitySheet,0,0,960,300,Color{r=11,g=8,b=6,a=255})
for i=1,3 do cell(utilitySheet,20+(i-1)*118,18,104,104,locks[i],1.65,C.brassDark) end
for i=1,2 do cell(utilitySheet,390+(i-1)*145,18,130,104,phases[i],1.35,C.brassDark) end
for i=1,3 do cell(utilitySheet,690+(i-1)*88,18,76,104,nav[i],1.7,i==1 and C.cyanDark or C.redDark) end
for i=1,6 do cell(utilitySheet,16+(i-1)*155,145,140,135,restrictions[i],1.55,C.brassDark) end
utilitySheet:saveAs(previewDir.."/icon_overhaul_utility_contact_sheet_960x300_0_5_0.png"); utilitySheet:saveAs(outputDir.."/icon_overhaul_utility_contact_sheet_960x300_0_5_0.png")

local full=img(1280,720); fill(full,0,0,1280,720,Color{r=8,g=6,b=5,a=255})
full:drawImage(nearest(itemSheet,960,270),Point(20,20)); full:drawImage(nearest(statusSheet,960,260),Point(20,305)); full:drawImage(nearest(utilitySheet,840,263),Point(420,437))
-- animation strips at their native pixel scale enlarged 2x
cell(full,1000,30,250,110,nearest(thinkingSheet,192,24),1,C.brassDark)
cell(full,1000,160,250,110,nearest(expireSheet,160,20),1,C.brassDark)
full:saveAs(previewDir.."/icon_overhaul_full_contact_sheet_1280x720_0_5_0.png"); full:saveAs(outputDir.."/icon_overhaul_full_contact_sheet_1280x720_0_5_0.png")

print("Icon overhaul 0.5.0 generated")
