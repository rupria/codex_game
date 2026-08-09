-- Shop ammo + standalone community-card reveal 0.3.3
-- Generated with Aseprite for deterministic pixel-art handoff.

local p = app.params
local sourceDir = assert(p.sourceDir)
local previewDir = assert(p.previewDir)
local outputDir = assert(p.outputDir)
local shopRuntimeDir = assert(p.shopRuntimeDir)
local halliRuntimeDir = assert(p.halliRuntimeDir)
local pokerRuntimeDir = assert(p.pokerRuntimeDir)

local C = {
  transparent=Color{r=0,g=0,b=0,a=0},
  ink=Color{r=4,g=4,b=3,a=255},
  panel=Color{r=8,g=8,b=7,a=255},
  brassDark=Color{r=83,g=45,b=12,a=255},
  brass=Color{r=178,g=108,b=24,a=255},
  brassHi=Color{r=255,g=202,b=82,a=255},
  copperDark=Color{r=83,g=34,b=20,a=255},
  copper=Color{r=190,g=85,b=38,a=255},
  copperHi=Color{r=248,g=151,b=71,a=255},
  steel=Color{r=82,g=73,b=59,a=255},
  cream=Color{r=246,g=231,b=184,a=255},
  glow=Color{r=255,g=181,b=62,a=255},
}

local function loadImage(path)
  local spr=app.open(path)
  assert(spr,"cannot open "..path)
  local img=Image(spr.cels[1].image)
  spr:close()
  return img
end

local function fill(img,x,y,w,h,color)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,color) end end
end

local function hline(img,x0,x1,y,color) fill(img,x0,y,x1-x0+1,1,color) end
local function vline(img,x,y0,y1,color) fill(img,x,y0,1,y1-y0+1,color) end

local function stroke(img,x,y,w,h,color,t)
  t=t or 1
  fill(img,x,y,w,t,color); fill(img,x,y+h-t,w,t,color)
  fill(img,x,y,t,h,color); fill(img,x+w-t,y,t,h,color)
end

local function resizeNearest(src,w,h)
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

local function copyRect(src,dst,sx,sy,w,h,dx,dy)
  for y=0,h-1 do
    for x=0,w-1 do
      local ux,uy=sx+x,sy+y; local tx,ty=dx+x,dy+y
      if ux>=0 and uy>=0 and ux<src.width and uy<src.height and tx>=0 and ty>=0 and tx<dst.width and ty<dst.height then
        dst:drawPixel(tx,ty,src:getPixel(ux,uy))
      end
    end
  end
end

local function rotate180(src)
  local dst=Image(src.width,src.height,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,src.height-1 do for x=0,src.width-1 do dst:drawPixel(src.width-1-x,src.height-1-y,src:getPixel(x,y)) end end
  return dst
end

local function line(img,x0,y0,x1,y1,color)
  x0=math.floor(x0); y0=math.floor(y0); x1=math.floor(x1); y1=math.floor(y1)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1
  local err=dx+dy
  while true do
    if x0>=0 and y0>=0 and x0<img.width and y0<img.height then img:drawPixel(x0,y0,color) end
    if x0==x1 and y0==y1 then break end
    local e2=2*err
    if e2>=dy then err=err+dy; x0=x0+sx end
    if e2<=dx then err=err+dx; y0=y0+sy end
  end
end


local function fillPoly(img,pts,color)
  local minY,maxY=pts[1][2],pts[1][2]
  for _,pt in ipairs(pts) do minY=math.min(minY,pt[2]); maxY=math.max(maxY,pt[2]) end
  for y=math.floor(minY),math.floor(maxY) do
    local xs={}
    for i=1,#pts do
      local a=pts[i]; local b=pts[(i % #pts)+1]
      if (y>=math.min(a[2],b[2])) and (y<math.max(a[2],b[2])) and a[2]~=b[2] then
        xs[#xs+1]=a[1]+(y-a[2])*(b[1]-a[1])/(b[2]-a[2])
      end
    end
    table.sort(xs)
    for i=1,#xs-1,2 do fill(img,math.ceil(xs[i]),y,math.floor(xs[i+1])-math.ceil(xs[i])+1,1,color) end
  end
end

local function strokePoly(img,pts,color)
  for i=1,#pts do local a=pts[i]; local b=pts[(i % #pts)+1]; line(img,a[1],a[2],b[1],b[2],color) end
end

local function setSpriteImage(spr,img)
  spr.cels[1].image:clear(C.transparent)
  spr.cels[1].image:drawImage(img,Point(0,0))
end

local function savePng(img,path)
  local spr=Sprite(img.width,img.height,ColorMode.RGB)
  setSpriteImage(spr,img)
  spr:saveAs(path)
  spr:close()
end

local function bulletImage(state)
  local img=Image(24,40,ColorMode.RGB); img:clear(C.transparent)
  -- Dark one-pixel silhouette.
  fill(img,10,0,4,1,C.ink); fill(img,8,1,8,1,C.ink); fill(img,7,2,10,2,C.ink)
  fill(img,6,4,12,10,C.ink); fill(img,5,14,14,19,C.ink)
  fill(img,3,33,18,5,C.ink); fill(img,6,38,12,2,C.ink)
  -- Copper projectile with a distinct rounded nose and cannelure.
  fill(img,10,1,4,1,C.copperHi); fill(img,8,2,8,2,C.copper)
  fill(img,7,4,10,7,C.copper); fill(img,8,4,3,6,C.copperHi)
  fill(img,7,11,10,2,C.copperDark); fill(img,8,13,8,1,C.steel)
  -- Brass casing, side highlight, base rim and primer cue.
  fill(img,6,14,12,18,C.brass)
  fill(img,7,15,3,15,C.brassHi); fill(img,15,15,3,16,C.brassDark)
  hline(img,6,17,22,C.brassHi); hline(img,6,17,31,C.brassDark)
  fill(img,4,33,16,4,C.brass); hline(img,4,19,33,C.brassHi)
  fill(img,7,37,10,1,C.brassDark); fill(img,9,38,6,1,C.steel)
  if state=="shine" then
    fill(img,9,3,2,5,Color{r=255,g=220,b=139,a=255})
    fill(img,7,16,2,9,Color{r=255,g=230,b=129,a=255})
  elseif state=="low" then
    fill(img,15,5,2,6,Color{r=94,g=35,b=24,a=255})
    fill(img,15,23,3,8,Color{r=89,g=50,b=18,a=255})
  end
  return img
end

local function updatedAmmoPanel(oldPanel,bullet)
  local img=Image(oldPanel)
  -- Preserve the existing shop frame and replace only the flat bullet pictogram.
  fill(img,8,7,42,44,C.panel)
  img:drawImage(bullet,Point(16,9))
  return img
end

local function makeBulletSource(path)
  local states={"idle","shine","low"}
  local spr=Sprite(24,40,ColorMode.RGB)
  spr.layers[1].name="realistic_revolver_cartridge"
  setSpriteImage(spr,bulletImage(states[1]))
  spr.frames[1].duration=0.25
  for i=2,#states do
    local fr=spr:newEmptyFrame()
    spr:newCel(spr.layers[1],fr,bulletImage(states[i]),Point(0,0))
    fr.duration=0.25
  end
  spr:saveAs(path); spr:close()
end

local function makeRevealBackdrop(base)
  local img=Image(base.width,base.height,ColorMode.RGB)
  local pc=app.pixelColor
  local cx,cy=base.width/2,base.height/2
  for y=0,base.height-1 do
    for x=0,base.width-1 do
      local px=base:getPixel(x,y)
      local r=pc.rgbaR(px); local g=pc.rgbaG(px); local b=pc.rgbaB(px); local a=pc.rgbaA(px)
      local nx=(x-cx)/(base.width*0.50); local ny=(y-cy)/(base.height*0.62)
      local dist=math.sqrt(nx*nx+ny*ny)
      local spot=math.max(0,1-dist)
      local quant=math.floor(spot*5)/5
      local factor=0.32+quant*0.68
      local warm=math.floor(quant*26)
      local rr=math.min(255,math.floor(r*factor+warm))
      local gg=math.min(255,math.floor(g*factor+warm*0.60))
      local bb=math.min(255,math.floor(b*factor+warm*0.16))
      img:drawPixel(x,y,Color{r=rr,g=gg,b=bb,a=a})
    end
  end
  return img
end

local function glowImage(size)
  local img=Image(size,size,ColorMode.RGB); img:clear(C.transparent)
  local c=(size-1)/2
  for y=0,size-1 do
    for x=0,size-1 do
      local dx=(x-c)/c; local dy=(y-c)/c
      local d=math.sqrt(dx*dx+dy*dy)
      if d<1 then
        local level=math.floor((1-d)*5)
        local alpha=math.max(0,math.min(105,level*18))
        img:drawPixel(x,y,Color{r=255,g=176,b=50,a=alpha})
      end
    end
  end
  hline(img,math.floor(c)-42,math.floor(c)+42,math.floor(c),Color{r=255,g=211,b=109,a=150})
  vline(img,math.floor(c),math.floor(c)-42,math.floor(c)+42,Color{r=255,g=211,b=109,a=150})
  return img
end

local function drawCornerBrackets(img,x,y,w,h,color)
  hline(img,x,x+18,y,color); vline(img,x,y,y+18,color)
  hline(img,x+w-19,x+w-1,y,color); vline(img,x+w-1,y,y+18,color)
  hline(img,x,x+18,y+h-1,color); vline(img,x,y+h-19,y+h-1,color)
  hline(img,x+w-19,x+w-1,y+h-1,color); vline(img,x+w-1,y+h-19,y+h-1,color)
end

local function revealFrame(bg,back,front,glow,index)
  local img=Image(bg)
  local sizes={
    {46,65,"back"},{72,101,"back"},{7,140,"back"},
    {38,150,"front"},{96,135,"front"},{128,180,"front"}
  }
  local glowSizes={82,112,146,164,196,232}
  local gs=glowSizes[index]
  img:drawImage(resizeNearest(glow,gs,gs),Point(math.floor((480-gs)/2),math.floor((270-gs)/2)))
  local s=sizes[index]
  local src=s[3]=="back" and back or front
  local card=resizeNearest(src,s[1],s[2])
  local x=math.floor((480-s[1])/2); local y=math.floor((270-s[2])/2)
  fill(img,x+4,y+5,s[1],s[2],Color{r=0,g=0,b=0,a=115})
  img:drawImage(card,Point(x,y))
  if index>=5 then drawCornerBrackets(img,x-12,y-12,s[1]+24,s[2]+24,C.brassHi) end
  if index==6 then
    local pts={{x-28,y+25},{x+s[1]+24,y+42},{x-22,y+s[2]-24},{x+s[1]+29,y+s[2]-34},{x-38,y+90},{x+s[1]+36,y+110}}
    for _,pt in ipairs(pts) do
      fill(img,pt[1],pt[2],3,3,C.brassHi)
      fill(img,pt[1]+1,pt[2]-2,1,7,C.cream)
      fill(img,pt[1]-2,pt[2]+1,7,1,C.cream)
    end
  end
  return img
end

local function makeRevealSource(frames,path)
  local durations={0.18,0.18,0.10,0.10,0.18,0.55}
  local spr=Sprite(480,270,ColorMode.RGB)
  spr.layers[1].name="standalone_community_reveal"
  setSpriteImage(spr,frames[1]); spr.frames[1].duration=durations[1]
  for i=2,#frames do
    local fr=spr:newEmptyFrame()
    spr:newCel(spr.layers[1],fr,frames[i],Point(0,0)); fr.duration=durations[i]
  end
  spr:saveAs(path); spr:close()
end

local function crateBrand(img,cx,cy)
  fill(img,cx-1,cy-7,3,15,C.brassDark); fill(img,cx-7,cy-1,15,3,C.brassDark)
  line(img,cx-5,cy-5,cx+5,cy+5,C.brassDark); line(img,cx+5,cy-5,cx-5,cy+5,C.brassDark)
  fill(img,cx-2,cy-2,5,5,Color{r=105,g=55,b=18,a=255})
end

local function itemCrate(state)
  local img=Image(112,96,ColorMode.RGB); img:clear(C.transparent)
  local outline=Color{r=24,g=12,b=7,a=255}
  local woodDark=Color{r=76,g=36,b=15,a=255}
  local wood=Color{r=139,g=72,b=29,a=255}
  local woodHi=Color{r=193,g=112,b=48,a=255}
  local woodGlow=Color{r=226,g=143,b=65,a=255}
  local front={{15,35},{99,35},{91,85},{22,85}}
  local top={{12,31},{56,9},{102,29},{58,51}}
  local inner={{23,31},{56,16},{91,30},{58,43}}
  local shadow={{10,38},{97,38},{103,86},{18,91}}

  fillPoly(img,shadow,Color{r=0,g=0,b=0,a=120})
  fillPoly(img,front,wood); strokePoly(img,front,outline)
  fillPoly(img,top,state=="closed" and wood or woodDark); strokePoly(img,top,outline)

  if state=="closed" then
    line(img,23,27,68,15,woodHi); line(img,38,39,84,21,woodDark)
    line(img,56,10,58,49,outline); line(img,18,32,58,49,woodHi); line(img,58,49,98,30,woodHi)
  else
    fillPoly(img,inner,Color{r=18,g=11,b=7,a=255}); strokePoly(img,inner,woodHi)
    -- Inner slats catch the warm table light.
    line(img,28,31,57,20,Color{r=61,g=30,b=15,a=255})
    line(img,57,20,86,30,Color{r=61,g=30,b=15,a=255})
    if state=="filled" then
      -- Tonic bottle, cloth/charm and spare cartridge peeking from the crate.
      fill(img,47,16,12,3,outline); fill(img,45,19,16,22,Color{r=12,g=82,b=87,a=255})
      fill(img,48,21,5,13,Color{r=74,g=213,b=203,a=255}); fill(img,46,35,14,5,outline)
      fill(img,29,24,13,14,Color{r=123,g=27,b=31,a=255}); fill(img,31,22,7,4,Color{r=239,g=63,b=59,a=255})
      fill(img,72,20,7,20,C.brass); fill(img,74,21,2,15,C.brassHi); fill(img,71,18,9,4,C.copper)
    end
    -- Chunky top rim.
    line(img,12,31,56,9,woodGlow); line(img,56,9,102,29,woodHi)
    line(img,102,29,58,51,woodDark); line(img,58,51,12,31,woodHi)
    line(img,18,32,56,13,C.brassDark); line(img,56,13,96,30,C.brassDark)
  end

  -- Front planks and ironed corners.
  line(img,18,50,96,50,woodDark); line(img,20,67,93,67,woodDark)
  line(img,19,52,95,52,woodHi); line(img,21,69,92,69,woodHi)
  fill(img,20,39,4,41,outline); fill(img,89,39,4,41,outline)
  fill(img,24,43,3,33,woodHi); fill(img,86,43,3,33,woodDark)
  line(img,31,58,48,55,Color{r=91,g=42,b=17,a=255})
  line(img,65,74,83,71,Color{r=91,g=42,b=17,a=255})
  line(img,38,79,55,76,Color{r=188,g=101,b=40,a=255})
  crateBrand(img,57,63)
  return img
end

local function makeCrateSource(states,path)
  local spr=Sprite(112,96,ColorMode.RGB)
  spr.layers[1].name="western_item_crate_states"
  setSpriteImage(spr,states[1]); spr.frames[1].duration=0.25
  for i=2,#states do
    local fr=spr:newEmptyFrame(); spr:newCel(spr.layers[1],fr,states[i],Point(0,0)); fr.duration=0.25
  end
  spr:saveAs(path); spr:close()
end

local oldPanel=loadImage(assert(p.oldAmmoPanel))
local shopPreview=loadImage(assert(p.shopPreview))
local tableBase=loadImage(assert(p.tableBase))
local cardBack=loadImage(assert(p.cardBack))
local cardFront=loadImage(assert(p.cardFront))
local pokerPreview=loadImage(assert(p.pokerPreview))

local bullet=bulletImage("idle")
local bulletShine=bulletImage("shine")
local bulletLow=bulletImage("low")
local ammoPanel=updatedAmmoPanel(oldPanel,bullet)
local newShop=Image(shopPreview); newShop:drawImage(ammoPanel,Point(40,28))

local revealBackdrop=makeRevealBackdrop(tableBase)
local bgSmall=resizeNearest(revealBackdrop,480,270)
local glow=glowImage(256)
local frames={}
for i=1,6 do frames[i]=revealFrame(bgSmall,cardBack,cardFront,glow,i) end
local crateClosed=itemCrate("closed")
local crateEmpty=itemCrate("empty")
local crateFilled=itemCrate("filled")

savePng(bullet,shopRuntimeDir.."/bar_shop_bullet_realistic_24x40_0_3_3.png")
savePng(bulletShine,shopRuntimeDir.."/bar_shop_bullet_realistic_shine_24x40_0_3_3.png")
savePng(bulletLow,shopRuntimeDir.."/bar_shop_bullet_realistic_low_24x40_0_3_3.png")
savePng(ammoPanel,shopRuntimeDir.."/bar_shop_ammo_panel_200x58_0_3_3.png")
savePng(revealBackdrop,halliRuntimeDir.."/community_reveal_backdrop_960x540_0_3_3.png")
savePng(glow,halliRuntimeDir.."/community_reveal_glow_256_0_3_3.png")
savePng(crateClosed,pokerRuntimeDir.."/poker_item_crate_closed_112x96_0_3_3.png")
savePng(crateEmpty,pokerRuntimeDir.."/poker_item_crate_empty_112x96_0_3_3.png")
savePng(crateFilled,pokerRuntimeDir.."/poker_item_crate_filled_112x96_0_3_3.png")

savePng(bullet,outputDir.."/bar_shop_bullet_realistic_24x40_0_3_3.png")
savePng(bulletShine,outputDir.."/bar_shop_bullet_realistic_shine_24x40_0_3_3.png")
savePng(bulletLow,outputDir.."/bar_shop_bullet_realistic_low_24x40_0_3_3.png")
savePng(ammoPanel,outputDir.."/bar_shop_ammo_panel_200x58_0_3_3.png")
savePng(newShop,previewDir.."/bar_shop_realistic_ammo_preview_960x540_0_3_3.png")
savePng(newShop,outputDir.."/bar_shop_realistic_ammo_preview_960x540_0_3_3.png")
savePng(revealBackdrop,outputDir.."/community_reveal_backdrop_960x540_0_3_3.png")
savePng(glow,outputDir.."/community_reveal_glow_256_0_3_3.png")
savePng(crateClosed,outputDir.."/poker_item_crate_closed_112x96_0_3_3.png")
savePng(crateEmpty,outputDir.."/poker_item_crate_empty_112x96_0_3_3.png")
savePng(crateFilled,outputDir.."/poker_item_crate_filled_112x96_0_3_3.png")

local sheet=Image(1440,540,ColorMode.RGB); sheet:clear(C.ink)
for i=1,6 do
  local col=(i-1)%3; local row=math.floor((i-1)/3)
  sheet:drawImage(frames[i],Point(col*480,row*270))
end
savePng(sheet,previewDir.."/community_card_reveal_storyboard_1440x540_0_3_3.png")
savePng(sheet,outputDir.."/community_card_reveal_storyboard_1440x540_0_3_3.png")

local finalReveal=resizeNearest(frames[6],960,540)
savePng(finalReveal,previewDir.."/community_card_reveal_final_960x540_0_3_3.png")
savePng(finalReveal,outputDir.."/community_card_reveal_final_960x540_0_3_3.png")

local compare=Image(1920,540,ColorMode.RGB); compare:clear(C.ink)
compare:drawImage(newShop,Point(0,0)); compare:drawImage(finalReveal,Point(960,0))
vline(compare,959,0,539,C.brassHi); vline(compare,960,0,539,C.brassHi)
savePng(compare,previewDir.."/shop_and_community_reveal_1920x540_0_3_3.png")
savePng(compare,outputDir.."/shop_and_community_reveal_1920x540_0_3_3.png")

local crateSheet=Image(336,96,ColorMode.RGB); crateSheet:clear(C.transparent)
crateSheet:drawImage(crateClosed,Point(0,0)); crateSheet:drawImage(crateEmpty,Point(112,0)); crateSheet:drawImage(crateFilled,Point(224,0))
savePng(crateSheet,previewDir.."/poker_item_crate_states_336x96_0_3_3.png")
savePng(crateSheet,outputDir.."/poker_item_crate_states_336x96_0_3_3.png")

local pokerNew=Image(pokerPreview)
copyRect(tableBase,pokerNew,620,88,160,140,620,88)
copyRect(tableBase,pokerNew,620,325,160,150,620,325)
pokerNew:drawImage(crateClosed,Point(642,100))
pokerNew:drawImage(crateFilled,Point(642,346))
savePng(pokerNew,previewDir.."/poker_item_crate_application_preview_960x540_0_3_3.png")
savePng(pokerNew,outputDir.."/poker_item_crate_application_preview_960x540_0_3_3.png")

local overview=Image(2880,540,ColorMode.RGB); overview:clear(C.ink)
overview:drawImage(newShop,Point(0,0)); overview:drawImage(finalReveal,Point(960,0)); overview:drawImage(pokerNew,Point(1920,0))
vline(overview,959,0,539,C.brassHi); vline(overview,960,0,539,C.brassHi)
vline(overview,1919,0,539,C.brassHi); vline(overview,1920,0,539,C.brassHi)
savePng(overview,previewDir.."/shop_reveal_poker_ui_overview_2880x540_0_3_3.png")
savePng(overview,outputDir.."/shop_reveal_poker_ui_overview_2880x540_0_3_3.png")

makeBulletSource(sourceDir.."/bar_shop_bullet_realistic_states_0_3_3.aseprite")
makeRevealSource(frames,sourceDir.."/community_card_reveal_sequence_0_3_3.aseprite")
makeCrateSource({crateClosed,crateEmpty,crateFilled},sourceDir.."/poker_item_crate_states_0_3_3.aseprite")

print("Shop_CommunityReveal_0_3_3 generated")
