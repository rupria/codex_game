-- Western skeletal Joker pair 0.07
-- Two distinct silhouettes: Brass Sheriff and Crimson Outlaw.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local sourceDir=assert(p.sourceDir)

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=6,g=7,b=7,a=255},
  nearBlack=Color{r=14,g=15,b=16,a=255}, boneDark=Color{r=133,g=117,b=87,a=255},
  bone=Color{r=232,g=216,b=170,a=255}, boneHi=Color{r=255,g=242,b=199,a=255},
  brassDark=Color{r=96,g=54,b=14,a=255}, brass=Color{r=201,g=128,b=28,a=255},
  brassHi=Color{r=255,g=203,b=71,a=255}, cyanDark=Color{r=0,g=71,b=83,a=255},
  cyan=Color{r=0,g=184,b=198,a=255}, cyanHi=Color{r=91,g=248,b=239,a=255},
  redDark=Color{r=102,g=18,b=28,a=255}, red=Color{r=197,g=39,b=52,a=255},
  redHi=Color{r=255,g=91,b=81,a=255}, leather=Color{r=89,g=49,b=28,a=255},
  leatherHi=Color{r=155,g=88,b=42,a=255}
}

local function loadImage(path)
  local spr=app.open(path); assert(spr,"cannot open "..path)
  local img=Image(spr.cels[1].image); spr:close(); return img
end

local function fill(img,x,y,w,h,color)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,color) end end
end

local function hline(img,x0,x1,y,color) fill(img,x0,y,x1-x0+1,1,color) end
local function vline(img,x,y0,y1,color) fill(img,x,y0,1,y1-y0+1,color) end

local function line(img,x0,y0,x1,y1,color)
  local dx=math.abs(x1-x0); local sx=x0<x1 and 1 or -1
  local dy=-math.abs(y1-y0); local sy=y0<y1 and 1 or -1; local err=dx+dy
  while true do
    img:drawPixel(x0,y0,color); if x0==x1 and y0==y1 then break end
    local e2=2*err; if e2>=dy then err=err+dy; x0=x0+sx end; if e2<=dx then err=err+dx; y0=y0+sy end
  end
end

local function resizeNearest(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h)); for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end end
  return dst
end

local function setSpriteImage(spr,img)
  spr.cels[1].image:clear(C.transparent); spr.cels[1].image:drawImage(img,Point(0,0))
end

local function savePng(img,path)
  local spr=Sprite(img.width,img.height,ColorMode.RGB); setSpriteImage(spr,img); spr:saveAs(path); spr:close()
end

local function drawJ(img,x,y,color)
  local px={{0,0},{1,0},{2,0},{3,0},{3,1},{3,2},{3,3},{3,4},{2,5},{1,6},{0,6},{0,5}}
  for _,pt in ipairs(px) do img:drawPixel(x+pt[1],y+pt[2],color) end
end

local function drawStar(img,cx,cy,color,dark)
  fill(img,cx-1,cy-4,3,9,dark); fill(img,cx-4,cy-1,9,3,dark)
  line(img,cx-3,cy-3,cx+3,cy+3,dark); line(img,cx+3,cy-3,cx-3,cy+3,dark)
  fill(img,cx-1,cy-3,3,7,color); fill(img,cx-3,cy-1,7,3,color)
  img:drawPixel(cx,cy,color)
end

local function drawHorseshoe(img,cx,cy,color)
  local pts={{-3,-3},{-4,-2},{-4,-1},{-4,0},{-3,1},{-3,2},{-2,3},{-1,3},{0,3},{1,3},{2,2},{3,1},{4,0},{4,-1},{4,-2},{3,-3}}
  for _,pt in ipairs(pts) do img:drawPixel(cx+pt[1],cy+pt[2],color) end
  img:drawPixel(cx-2,cy-2,C.ink); img:drawPixel(cx+2,cy-2,C.ink)
end

local function paperBase(template)
  local img=Image(56,78,ColorMode.RGB)
  img:clear(C.transparent)
  local cream=template:getPixel(28,8)
  -- Rebuild the established beveled card silhouette without inherited face art.
  fill(img,5,4,49,73,Color{r=8,g=12,b=15,a=255})
  fill(img,2,1,50,74,C.ink)
  fill(img,3,2,48,72,cream)
  img:drawPixel(3,2,C.ink); img:drawPixel(50,2,C.ink)
  img:drawPixel(3,73,C.ink); img:drawPixel(50,73,C.ink)
  fill(img,1,4,2,67,C.ink); fill(img,51,4,2,67,C.ink)
  fill(img,4,0,45,1,C.ink); fill(img,4,75,45,1,C.ink)
  local paper=Color{r=228,g=211,b=168,a=255}
  local flecks={{8,18},{46,18},{11,69},{44,66},{26,12},{31,72},{6,48},{49,45}}
  for _,pt in ipairs(flecks) do img:drawPixel(pt[1],pt[2],paper) end
  hline(img,5,48,3,Color{r=214,g=196,b=151,a=255})
  vline(img,4,5,71,Color{r=214,g=196,b=151,a=255})
  return img
end

local function skull(img,x,y)
  -- 14x15 skull with asymmetrical Joker eyes.
  fill(img,x+3,y,8,1,C.boneDark); fill(img,x+1,y+1,12,2,C.bone)
  fill(img,x,y+3,14,7,C.bone); fill(img,x+2,y+10,10,3,C.bone)
  fill(img,x+4,y+13,6,2,C.boneDark)
  fill(img,x+2,y+4,4,4,C.ink); fill(img,x+8,y+4,4,4,C.ink)
  img:drawPixel(x+4,y+5,C.cyanHi); img:drawPixel(x+10,y+5,C.redHi)
  fill(img,x+6,y+8,2,3,C.ink)
  img:drawPixel(x+4,y+11,C.ink); img:drawPixel(x+6,y+12,C.ink); img:drawPixel(x+8,y+12,C.ink); img:drawPixel(x+10,y+11,C.ink)
end

local function brassSheriff(template)
  local img=paperBase(template)
  drawJ(img,6,6,C.ink); drawStar(img,46,9,C.brassHi,C.ink)
  -- Arched portrait frame.
  hline(img,13,42,22,C.brassDark); vline(img,12,23,67,C.brassDark); vline(img,43,23,67,C.brassDark)
  hline(img,15,40,69,C.brassDark); img:drawPixel(13,21,C.brass); img:drawPixel(42,21,C.brass)
  -- Broad ivory lawman hat, distinct from the outlaw silhouette.
  fill(img,21,19,14,2,C.ink); fill(img,17,21,22,2,C.ink); fill(img,13,23,30,3,C.ink)
  fill(img,22,18,12,3,C.bone); fill(img,18,21,20,2,C.boneHi); fill(img,15,23,26,1,C.bone)
  fill(img,21,21,14,2,C.brass); fill(img,23,20,10,1,C.brassHi)
  skull(img,21,26)
  -- Cyan neckerchief and tan duster.
  fill(img,24,41,8,5,C.cyanDark); fill(img,26,42,4,5,C.cyanHi)
  fill(img,16,45,10,20,C.leather); fill(img,30,45,10,20,C.leather)
  fill(img,13,49,5,15,C.leatherHi); fill(img,38,49,5,15,C.leatherHi)
  line(img,18,47,25,64,C.brass); line(img,37,47,30,64,C.brass)
  fill(img,25,47,6,18,C.nearBlack); drawStar(img,28,55,C.brassHi,C.brassDark)
  -- Revolver handles distinguish the sheriff pose.
  line(img,15,57,11,47,C.ink); line(img,40,57,44,47,C.ink)
  fill(img,9,45,5,3,C.steel or C.boneDark); fill(img,42,45,5,3,C.boneDark)
  hline(img,17,39,67,C.ink)
  return img
end

local function crimsonOutlaw(template)
  local img=paperBase(template)
  drawJ(img,6,6,C.red); drawHorseshoe(img,46,9,C.red)
  -- Tilted black hat and torn red band: a different silhouette and posture.
  fill(img,19,18,20,2,C.ink); fill(img,16,20,26,2,C.nearBlack); fill(img,10,22,35,3,C.ink)
  fill(img,22,16,15,3,C.nearBlack); fill(img,24,14,10,3,C.ink)
  line(img,16,21,39,18,C.redDark); line(img,20,21,37,19,C.redHi)
  skull(img,20,25)
  -- Eye patch and bandana create an outlaw identity.
  line(img,21,30,33,27,C.ink); fill(img,22,34,13,5,C.redDark)
  img:drawPixel(24,35,C.redHi); img:drawPixel(32,36,C.redHi)
  -- Split poncho: crimson left, near-black right, card-fan shoulder accents.
  fill(img,13,43,15,23,C.redDark); fill(img,28,43,15,23,C.nearBlack)
  fill(img,10,48,6,16,C.red); fill(img,40,48,6,16,C.ink)
  line(img,14,45,27,65,C.redHi); line(img,42,45,29,65,C.brass)
  fill(img,24,45,8,20,C.leather); fill(img,26,47,4,14,C.leatherHi)
  -- Fan of marked cards in one hand.
  fill(img,8,51,6,9,C.boneHi); fill(img,10,49,6,9,C.bone); fill(img,12,47,6,9,C.boneHi)
  img:drawPixel(14,49,C.red); img:drawPixel(12,51,C.ink); img:drawPixel(10,53,C.red)
  -- Single revolver on the opposite side.
  line(img,42,55,47,47,C.boneDark); fill(img,45,45,5,3,C.ink)
  hline(img,16,41,67,C.ink)
  return img
end

local template=loadImage(assert(p.template))
local sheriff=brassSheriff(template)
local outlaw=crimsonOutlaw(template)

savePng(sheriff,runtimeDir.."/card_poker_joker_brass_sheriff.png")
savePng(outlaw,runtimeDir.."/card_poker_joker_crimson_outlaw.png")
savePng(sheriff,outputDir.."/card_poker_joker_brass_sheriff.png")
savePng(outlaw,outputDir.."/card_poker_joker_crimson_outlaw.png")

local sheet=Image(112,78,ColorMode.RGB); sheet:clear(C.transparent)
sheet:drawImage(sheriff,Point(0,0)); sheet:drawImage(outlaw,Point(56,0))
savePng(sheet,previewDir.."/joker_pair_native_112x78_0_07.png")
savePng(sheet,outputDir.."/joker_pair_native_112x78_0_07.png")

local preview=Image(472,344,ColorMode.RGB); preview:clear(Color{r=5,g=12,b=13,a=255})
local largeSheriff=resizeNearest(sheriff,224,312); local largeOutlaw=resizeNearest(outlaw,224,312)
preview:drawImage(largeSheriff,Point(8,16)); preview:drawImage(largeOutlaw,Point(240,16))
vline(preview,235,16,327,C.brassHi); vline(preview,236,16,327,C.brassDark)
savePng(preview,previewDir.."/joker_pair_preview_472x344_0_07.png")
savePng(preview,outputDir.."/joker_pair_preview_472x344_0_07.png")

local spr=Sprite(56,78,ColorMode.RGB); spr.layers[1].name="joker_pair"
setSpriteImage(spr,sheriff); spr.frames[1].duration=0.25
local fr=spr:newEmptyFrame(); spr:newCel(spr.layers[1],fr,outlaw,Point(0,0)); fr.duration=0.25
spr:saveAs(sourceDir.."/western_joker_pair_0_07.aseprite"); spr:close()

print("Jokers_0_07 generated")
