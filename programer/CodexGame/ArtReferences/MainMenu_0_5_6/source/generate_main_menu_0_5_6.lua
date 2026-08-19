-- Main menu Western UI refresh for GitHub issue #62.
-- Keeps the approved saloon background and replaces the prototype crest/buttons.
-- Required params: base, runtimeRoot, referenceRoot, previewRoot

local p = app.params
local basePath = assert(p.base, "base is required")
local runtimeRoot = assert(p.runtimeRoot, "runtimeRoot is required")
local referenceRoot = assert(p.referenceRoot, "referenceRoot is required")
local previewRoot = assert(p.previewRoot, "previewRoot is required")

local pc = app.pixelColor
local function img(w,h)
  local out = Image(w,h,ColorMode.RGB)
  out:clear(Color{r=0,g=0,b=0,a=0})
  return out
end
local function fill(im,x,y,w,h,c)
  for yy=math.max(0,y),math.min(im.height-1,y+h-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function lineH(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function lineV(im,x,y,h,c,t) fill(im,x,y,t or 1,h,c) end
local function frame(im,x,y,w,h,c,t)
  t=t or 1
  lineH(im,x,y,w,c,t); lineH(im,x,y+h-t,w,c,t)
  lineV(im,x,y,h,c,t); lineV(im,x+w-t,y,h,c,t)
end
local function disc(im,cx,cy,r,c)
  for y=-r,r do
    local span=math.floor(math.sqrt(r*r-y*y))
    lineH(im,cx-span,cy+y,span*2+1,c,1)
  end
end
local function poly(im,pts,c)
  local minY,maxY=pts[1][2],pts[1][2]
  for _,pt in ipairs(pts) do minY=math.min(minY,pt[2]); maxY=math.max(maxY,pt[2]) end
  for y=minY,maxY do
    local nodes={}
    local j=#pts
    for i=1,#pts do
      local xi,yi=pts[i][1],pts[i][2]
      local xj,yj=pts[j][1],pts[j][2]
      if (yi<y and yj>=y) or (yj<y and yi>=y) then
        nodes[#nodes+1]=math.floor(xi+(y-yi)/(yj-yi)*(xj-xi))
      end
      j=i
    end
    table.sort(nodes)
    for i=1,#nodes-1,2 do lineH(im,nodes[i],y,nodes[i+1]-nodes[i]+1,c,1) end
  end
end
local function nearest(src,w,h)
  local out=img(w,h)
  for y=0,h-1 do
    local sy=math.min(src.height-1,math.floor(y*src.height/h))
    for x=0,w-1 do
      local sx=math.min(src.width-1,math.floor(x*src.width/w))
      out:drawPixel(x,y,src:getPixel(sx,sy))
    end
  end
  return out
end

local function removePrototypeAccent(im,x,y,w,h,replacement)
  for yy=y,y+h-1 do
    for xx=x,x+w-1 do
      local v=im:getPixel(xx,yy)
      local r,g,b=pc.rgbaR(v),pc.rgbaG(v),pc.rgbaB(v)
      local cyan=(g>35 and b>35 and g>r*1.35 and b>r*1.25)
      local red=(r>42 and r>g*1.45 and r>b*1.12)
      if cyan or red then im:drawPixel(xx,yy,replacement) end
    end
  end
end

local C={
  ink=Color{r=8,g=7,b=6,a=255}, shadow=Color{r=0,g=0,b=0,a=205},
  leather=Color{r=27,g=18,b=13,a=255}, leather2=Color{r=45,g=29,b=18,a=255},
  wood=Color{r=53,g=31,b=16,a=255}, woodHi=Color{r=85,g=49,b=23,a=255},
  brass0=Color{r=67,g=38,b=14,a=255}, brass1=Color{r=132,g=78,b=23,a=255},
  brass2=Color{r=205,g=139,b=43,a=255}, brass3=Color{r=247,g=196,b=91,a=255},
  bone=Color{r=232,g=217,b=174,a=255}, boneShade=Color{r=154,g=129,b=87,a=255},
  cyan=Color{r=55,g=205,b=194,a=255}, red=Color{r=214,g=60,b=51,a=255},
  green=Color{r=15,g=34,b=29,a=255}, table=Color{r=11,g=18,b=13,a=255},
  cream=Color{r=244,g=226,b=180,a=255}
}

local function bolt(im,x,y)
  disc(im,x,y,4,C.ink); disc(im,x,y,3,C.brass1); fill(im,x,y-2,1,3,C.brass3)
end

local function button(state,accent)
  local out=img(336,78)
  local y=(state=="pressed") and 5 or 2
  fill(out,10,y+6,316,66,C.shadow)
  poly(out,{{8,y+8},{16,y},{320,y},{328,y+8},{328,y+64},{320,y+72},{16,y+72},{8,y+64}},C.ink)
  poly(out,{{11,y+10},{18,y+3},{318,y+3},{325,y+10},{325,y+62},{318,y+69},{18,y+69},{11,y+62}},C.brass1)
  poly(out,{{15,y+12},{21,y+6},{315,y+6},{321,y+12},{321,y+60},{315,y+66},{21,y+66},{15,y+60}},C.leather)
  frame(out,22,y+12,292,50,C.brass0,2)
  lineH(out,31,y+16,274,C.woodHi,2)
  lineH(out,31,y+57,274,C.ink,2)
  -- Slim side accents retain player/AI readability without a full neon border.
  fill(out,14,y+29,4,16,accent); fill(out,318,y+29,4,16,accent)
  poly(out,{{18,y+37},{24,y+31},{30,y+37},{24,y+43}},accent)
  poly(out,{{306,y+37},{312,y+31},{318,y+37},{312,y+43}},accent)
  bolt(out,20,y+10); bolt(out,316,y+10); bolt(out,20,y+62); bolt(out,316,y+62)
  if state=="hover" then
    lineH(out,36,y+9,264,C.brass3,2)
    fill(out,43,y+14,32,2,C.brass2); fill(out,261,y+14,32,2,C.brass2)
  elseif state=="pressed" then
    fill(out,26,y+15,284,42,Color{r=13,g=10,b=8,a=90})
    lineH(out,40,y+58,256,C.brass2,2)
  end
  return out
end

local function drawCard(im,x,y,lean,accent)
  -- Compact, deliberately subdued duel-card silhouette.
  local pts
  if lean<0 then pts={{x+7,y},{x+57,y+7},{x+48,y+88},{x,y+81}}
  else pts={{x,y+7},{x+50,y},{x+57,y+81},{x+9,y+88}} end
  poly(im,pts,C.ink)
  local inner={}
  for _,pt in ipairs(pts) do inner[#inner+1]={pt[1]+((pt[1]<x+28) and 3 or -3),pt[2]+((pt[2]<y+44) and 3 or -3)} end
  poly(im,inner,C.green)
  if lean<0 then lineV(im,x+5,y+14,55,accent,3) else lineV(im,x+49,y+14,55,accent,3) end
end

local function crest()
  local out=img(240,170)
  -- Opaque leather sheriff plaque fully replaces the former prototype mark.
  poly(out,{{120,2},{193,18},{229,58},{229,113},{193,153},{120,168},{47,153},{11,113},{11,58},{47,18}},C.ink)
  poly(out,{{120,6},{190,21},{224,61},{224,110},{190,149},{120,164},{50,149},{16,110},{16,61},{50,21}},C.brass0)
  poly(out,{{120,10},{187,25},{219,63},{219,108},{187,145},{120,159},{53,145},{21,108},{21,63},{53,25}},C.leather)
  frame(out,57,24,126,4,C.brass1,1)
  drawCard(out,34,39,-1,C.cyan); drawCard(out,149,39,1,C.red)
  -- Crossed revolvers: strong diagonal silhouette, small brass catches.
  poly(out,{{52,115},{61,105},{116,61},{123,69},{70,116},{66,132},{55,132}},C.ink)
  poly(out,{{188,115},{179,105},{124,61},{117,69},{170,116},{174,132},{185,132}},C.ink)
  lineH(out,61,111,24,C.brass1,4); lineH(out,155,111,24,C.brass1,4)
  -- Sheriff star.
  poly(out,{{120,24},{135,56},{170,43},{154,76},{188,91},{151,101},{161,139},{129,119},{120,154},{111,119},{79,139},{89,101},{52,91},{86,76},{70,43},{105,56}},C.brass0)
  poly(out,{{120,31},{133,62},{162,51},{147,79},{178,91},{145,98},{154,130},{127,112},{120,143},{113,112},{86,130},{95,98},{62,91},{93,79},{78,51},{107,62}},C.brass2)
  disc(out,120,91,42,C.ink); disc(out,120,91,37,C.brass1); disc(out,120,91,33,C.leather)
  -- Bell body centered in the badge.
  poly(out,{{102,74},{108,61},{116,56},{124,56},{132,61},{138,74},{141,102},{148,109},{148,116},{92,116},{92,109},{99,102}},C.brass0)
  poly(out,{{106,76},{111,65},{117,61},{123,61},{129,65},{134,76},{136,101},{141,107},{99,107},{104,101}},C.brass2)
  lineH(out,97,107,46,C.brass3,4); disc(out,120,116,6,C.brass1)
  -- Skull mark within the bell.
  disc(out,120,85,14,C.bone); fill(out,108,84,24,13,C.bone)
  disc(out,114,84,4,C.ink); disc(out,126,84,4,C.ink)
  poly(out,{{120,89},{116,96},{124,96}},C.ink)
  fill(out,111,97,18,6,C.bone); lineV(out,115,99,6,C.ink,2); lineV(out,121,99,6,C.ink,2); lineV(out,127,99,6,C.ink,2)
  -- Highlights and rivets.
  fill(out,116,40,8,5,C.brass3)
  for _,pt in ipairs{{120,28},{183,91},{120,150},{57,91}} do bolt(out,pt[1],pt[2]) end
  return out
end

local glyphs={
  A={"01110","10001","10001","11111","10001","10001","10001"},
  D={"11110","10001","10001","10001","10001","10001","11110"},
  E={"11111","10000","10000","11110","10000","10000","11111"},
  G={"01110","10001","10000","10111","10001","10001","01111"},
  I={"11111","00100","00100","00100","00100","00100","11111"},
  R={"11110","10001","10001","11110","10100","10010","10001"},
  S={"01111","10000","10000","01110","00001","00001","11110"},
  T={"11111","00100","00100","00100","00100","00100","00100"},
  U={"10001","10001","10001","10001","10001","10001","01110"}
}
local function text(im,word,cx,y,scale,c)
  local total=#word*6*scale-scale
  local x0=math.floor(cx-total/2)
  for i=1,#word do
    local g=glyphs[word:sub(i,i)]
    if g then
      for yy,row in ipairs(g) do
        for xx=1,#row do if row:sub(xx,xx)=="1" then fill(im,x0+(i-1)*6*scale+(xx-1)*scale,y+(yy-1)*scale,scale,scale,c) end end
      end
    end
  end
end

local startIdle=button("idle",C.cyan)
local startHover=button("hover",C.cyan)
local startPressed=button("pressed",C.cyan)
local guideIdle=button("idle",C.red)
local guideHover=button("hover",C.red)
local guidePressed=button("pressed",C.red)
local emblem=crest()

startIdle:saveAs(runtimeRoot.."/main_menu_start_idle_336x78_0_5_6.png")
startHover:saveAs(runtimeRoot.."/main_menu_start_hover_336x78_0_5_6.png")
startPressed:saveAs(runtimeRoot.."/main_menu_start_pressed_336x78_0_5_6.png")
guideIdle:saveAs(runtimeRoot.."/main_menu_guide_idle_336x78_0_5_6.png")
guideHover:saveAs(runtimeRoot.."/main_menu_guide_hover_336x78_0_5_6.png")
guidePressed:saveAs(runtimeRoot.."/main_menu_guide_pressed_336x78_0_5_6.png")
emblem:saveAs(runtimeRoot.."/main_menu_duel_crest_240x170_0_5_6.png")

local base=assert(app.open(basePath),"cannot open "..basePath)
local screen=base.cels[1].image:clone()
base:close()

-- Clear only the former baked cyan/red accents. This preserves the table texture
-- instead of leaving a flat rectangular repair patch around the new UI.
removePrototypeAccent(screen,376,78,208,176,Color{r=4,g=6,b=6,a=255})
removePrototypeAccent(screen,306,258,348,180,C.table)
screen:drawImage(emblem,Point(360,82))
-- Exact legacy button rects: the existing runtime hit targets stay unchanged.
screen:drawImage(startIdle,Point(312,264))
screen:drawImage(guideIdle,Point(312,352))
screen:saveAs(runtimeRoot.."/start_screen_background_960x540_0_5_6.png")
-- Drop-in path used by PlayableDevSceneBuilder; preserves code and binding.
screen:saveAs(referenceRoot.."/start_screen_background_dropin_960x540_0_5_6.png")

local preview=screen:clone()
text(preview,"START",480,289,4,C.cream)
text(preview,"GUIDE",480,377,4,C.cream)
preview:saveAs(previewRoot.."/main_menu_western_ui_preview_960x540_0_5_6.png")

local states=img(960,360)
fill(states,0,0,960,360,Color{r=11,g=8,b=6,a=255})
local sets={{startIdle,"idle"},{startHover,"hover"},{startPressed,"pressed"},{guideIdle,"idle"},{guideHover,"hover"},{guidePressed,"pressed"}}
for i,pair in ipairs(sets) do
  local col=(i<=3) and 0 or 1
  local row=(i-1)%3
  local x=42+col*462; local y=22+row*108
  states:drawImage(pair[1],Point(x,y))
  text(states,(i<=3) and "START" or "GUIDE",x+168,y+25,3,C.cream)
end
states:saveAs(previewRoot.."/main_menu_button_states_960x360_0_5_6.png")

local src=Sprite(960,540,ColorMode.RGB)
src.layers[1].name="approved_main_menu_composite"
src.cels[1].image:drawImage(screen,Point(0,0))
src:saveAs(referenceRoot.."/main_menu_western_ui_960x540_0_5_6.aseprite")
src:close()

local buttonSrc=Sprite(336,78,ColorMode.RGB)
buttonSrc.layers[1].name="main_menu_button_states"
buttonSrc.cels[1].image:drawImage(startIdle,Point(0,0))
local all={startHover,startPressed,guideIdle,guideHover,guidePressed}
for _,state in ipairs(all) do
  local f=buttonSrc:newEmptyFrame()
  buttonSrc:newCel(buttonSrc.layers[1],f,state,Point(0,0))
end
buttonSrc:saveAs(referenceRoot.."/main_menu_button_states_336x78_0_5_6.aseprite")
buttonSrc:close()
