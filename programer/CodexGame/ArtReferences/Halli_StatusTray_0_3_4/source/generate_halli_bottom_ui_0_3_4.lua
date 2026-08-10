-- Halli bottom HUD 0.3.4
-- Aseprite generator: player-only acquired-card tray and application preview.

local p=app.params
local runtimeDir=assert(p.runtimeDir)
local sourceDir=assert(p.sourceDir)
local previewDir=assert(p.previewDir)
local outputDir=assert(p.outputDir)
local basePath=assert(p.base)
local cardAPath=assert(p.cardA)
local cardBPath=assert(p.cardB)

local C={
  transparent=Color{r=0,g=0,b=0,a=0}, ink=Color{r=3,g=5,b=5,a=255},
  shadow=Color{r=7,g=5,b=3,a=255}, deep=Color{r=4,g=12,b=14,a=250},
  felt=Color{r=5,g=24,b=28,a=250}, felt2=Color{r=8,g=35,b=39,a=250},
  brassDark=Color{r=69,g=39,b=12,a=255}, brass=Color{r=145,g=87,b=22,a=255},
  brassHi=Color{r=225,g=160,b=48,a=255}, cyanDark=Color{r=0,g=82,b=94,a=255},
  cyan=Color{r=0,g=226,b=232,a=255}, cyanHi=Color{r=137,g=255,b=249,a=255},
  table=Color{r=3,g=14,b=11,a=255}
}

local function fill(img,x,y,w,h,c)
  local x0=math.max(0,math.floor(x)); local y0=math.max(0,math.floor(y))
  local x1=math.min(img.width-1,math.floor(x+w-1)); local y1=math.min(img.height-1,math.floor(y+h-1))
  for yy=y0,y1 do for xx=x0,x1 do img:drawPixel(xx,yy,c) end end
end
local function stroke(img,x,y,w,h,c,t)
  t=t or 1; fill(img,x,y,w,t,c); fill(img,x,y+h-t,w,t,c); fill(img,x,y,t,h,c); fill(img,x+w-t,y,t,h,c)
end
local function hline(img,x0,x1,y,c) fill(img,x0,y,x1-x0+1,1,c) end
local function vline(img,x,y0,y1,c) fill(img,x,y0,1,y1-y0+1,c) end
local function diamond(img,cx,cy,r,c)
  for yy=-r,r do local s=r-math.abs(yy); hline(img,cx-s,cx+s,cy+yy,c) end
end
local function ring(img,cx,cy,r,c)
  for y=-r,r do for x=-r,r do
    local d=x*x+y*y
    if d<=r*r and d>=(r-2)*(r-2) then img:drawPixel(cx+x,cy+y,c) end
  end end
end
local function load(path)
  local s=app.open(path); assert(s,"cannot open "..path); local i=Image(s.cels[1].image); s:close(); return i
end
local function resize(src,w,h)
  local dst=Image(w,h,ColorMode.RGB); dst:clear(C.transparent)
  for y=0,h-1 do local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do local sx=math.min(src.width-1,math.floor(x*src.width/w)); dst:drawPixel(x,y,src:getPixel(sx,sy)) end
  end
  return dst
end
local function paste(src,dst,x,y) dst:drawImage(src,Point(x,y)) end
local function setImage(spr,img) spr.cels[1].image:clear(C.transparent); spr.cels[1].image:drawImage(img,Point(0,0)) end
local function save(img,path)
  local s=Sprite(img.width,img.height,ColorMode.RGB); setImage(s,img); s:saveAs(path); s:close()
end

local function trayBase()
  local img=Image(378,130,ColorMode.RGB); img:clear(C.transparent)
  fill(img,5,11,368,116,C.shadow)
  stroke(img,4,9,370,118,C.ink,3); stroke(img,7,12,364,112,C.brassDark,2)
  hline(img,12,365,16,C.brassHi); hline(img,12,365,119,C.brass)
  fill(img,11,19,356,98,C.deep); stroke(img,11,19,356,98,C.brassDark,1)

  -- Left side is one item socket, not a card slot and never a plus icon.
  fill(img,17,29,72,78,C.felt); stroke(img,17,29,72,78,C.cyanDark,2)
  ring(img,53,68,18,C.brassDark); ring(img,53,68,14,C.brassHi)
  ring(img,53,68,10,C.brass); diamond(img,53,68,3,C.shadow)
  hline(img,22,84,102,C.cyan,2)

  -- One continuous acquired-card lane. No baked empty slots or dividers.
  fill(img,101,27,258,82,C.felt); stroke(img,101,27,258,82,C.cyanDark,1)
  fill(img,105,31,250,70,C.felt2); hline(img,105,355,105,C.cyan,2)
  vline(img,96,25,111,C.brassDark); vline(img,97,25,111,C.brassHi)
  diamond(img,189,12,5,C.ink); diamond(img,189,12,3,C.cyan); diamond(img,189,12,1,C.cyanHi)
  return img
end

local function trayState(base,cards,count)
  local img=Image(base)
  for i=1,count do paste(cards[((i-1)%#cards)+1],img,112+(i-1)*50,29) end
  return img
end

local function makeSource(base,cards,path)
  local s=Sprite(378,130,ColorMode.RGB); s.layers[1].name="player_item_socket_and_open_card_lane"
  setImage(s,trayState(base,cards,0)); s.frames[1].duration=0.25
  for n=1,3 do local f=s:newEmptyFrame(); s:newCel(s.layers[1],f,trayState(base,cards,n),Point(0,0)); f.duration=0.25 end
  s:saveAs(path); s:close()
end

local base=load(basePath); local cardA=load(cardAPath); local cardB=load(cardBPath)
local cards={cardA,cardB}; local tray=trayBase()
save(tray,runtimeDir.."/player_acquired_tray_open_378x130_0_3_4.png")
save(tray,outputDir.."/player_acquired_tray_open_378x130_0_3_4.png")
makeSource(tray,cards,sourceDir.."/player_acquired_tray_states_0_3_4.aseprite")

local states=Image(1512,130,ColorMode.RGB); states:clear(C.transparent)
for n=0,3 do paste(trayState(tray,cards,n),states,n*378,0) end
save(states,previewDir.."/player_acquired_tray_states_0_3_4.png")
save(states,outputDir.."/player_acquired_tray_states_0_3_4.png")

local after=Image(base)
-- Replace the legacy four rigid slots with the player-only open tray.
fill(after,15,400,395,140,C.table)
paste(tray,after,24,406)
paste(cardA,after,136,435); paste(cardB,after,184,435)
-- Deliberately leave lower-right empty: AI deck is already top-center.
save(after,previewDir.."/halli_bottom_ui_application_preview_960x540_0_3_4.png")
save(after,outputDir.."/halli_bottom_ui_application_preview_960x540_0_3_4.png")

local compare=Image(1924,540,ColorMode.RGB); compare:clear(C.ink)
paste(base,compare,0,0); fill(compare,960,0,4,540,C.brassHi); paste(after,compare,964,0)
save(compare,previewDir.."/halli_bottom_ui_before_after_1924x540_0_3_4.png")
save(compare,outputDir.."/halli_bottom_ui_before_after_1924x540_0_3_4.png")
print("Halli_StatusTray_0_3_4 generated")
