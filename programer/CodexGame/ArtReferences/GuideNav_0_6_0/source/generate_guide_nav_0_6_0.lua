-- Guide navigation correction for GitHub issue #65.
-- Keeps GuideNav 0.5.6 geometry and states; removes the stray yellow shaft marks.
-- Required params: base, runtimeRoot, sourceRoot, previewRoot

local p=app.params
local basePath=assert(p.base,"base is required")
local runtimeRoot=assert(p.runtimeRoot,"runtimeRoot is required")
local sourceRoot=assert(p.sourceRoot,"sourceRoot is required")
local previewRoot=assert(p.previewRoot,"previewRoot is required")

local function image(w,h)
  local im=Image(w,h,ColorMode.RGB)
  im:clear(Color{r=0,g=0,b=0,a=0})
  return im
end
local function fill(im,x,y,w,hh,c)
  for yy=math.max(0,y),math.min(im.height-1,y+hh-1) do
    for xx=math.max(0,x),math.min(im.width-1,x+w-1) do im:drawPixel(xx,yy,c) end
  end
end
local function h(im,x,y,w,c,t) fill(im,x,y,w,t or 1,c) end
local function v(im,x,y,hh,c,t) fill(im,x,y,t or 1,hh,c) end
local function frame(im,x,y,w,hh,c,t)
  t=t or 1; h(im,x,y,w,c,t); h(im,x,y+hh-t,w,c,t); v(im,x,y,hh,c,t); v(im,x+w-t,y,hh,c,t)
end
local function disc(im,cx,cy,r,c)
  for y=-r,r do local s=math.floor(math.sqrt(r*r-y*y)); h(im,cx-s,cy+y,s*2+1,c,1) end
end
local function poly(im,pts,c)
  local minY,maxY=pts[1][2],pts[1][2]
  for _,pt in ipairs(pts) do minY=math.min(minY,pt[2]); maxY=math.max(maxY,pt[2]) end
  for y=minY,maxY do
    local nodes={}; local j=#pts
    for i=1,#pts do
      local xi,yi=pts[i][1],pts[i][2]; local xj,yj=pts[j][1],pts[j][2]
      if (yi<y and yj>=y) or (yj<y and yi>=y) then nodes[#nodes+1]=math.floor(xi+(y-yi)/(yj-yi)*(xj-xi)) end
      j=i
    end
    table.sort(nodes)
    for i=1,#nodes-1,2 do h(im,nodes[i],y,nodes[i+1]-nodes[i]+1,c,1) end
  end
end

local C={
  ink=Color{r=7,g=6,b=5,a=255}, shadow=Color{r=0,g=0,b=0,a=190},
  leather=Color{r=28,g=19,b=14,a=255}, brass0=Color{r=72,g=42,b=17,a=255},
  brass1=Color{r=132,g=79,b=25,a=255}, brass3=Color{r=244,g=192,b=81,a=255},
  cyan=Color{r=48,g=197,b=188,a=255}, red=Color{r=215,g=56,b=48,a=255},
  disabled=Color{r=67,g=61,b=51,a=255}, rail=Color{r=17,g=13,b=10,a=255}
}

local function bolt(im,x,y,c)
  disc(im,x,y,3,C.ink); disc(im,x,y,2,c or C.brass1); fill(im,x,y-1,1,2,C.brass3)
end

local function button(kind,state)
  local im=image(56,58)
  local down=(state=="pressed") and 2 or 0
  fill(im,4,5+down,50,50,C.shadow)
  poly(im,{{5,6+down},{11,1+down},{46,1+down},{52,7+down},{52,48+down},{46,54+down},{10,54+down},{5,49+down}},C.ink)
  poly(im,{{8,8+down},{13,4+down},{44,4+down},{49,9+down},{49,46+down},{44,51+down},{13,51+down},{8,46+down}},state=="disabled" and C.disabled or C.brass1)
  poly(im,{{11,10+down},{15,7+down},{42,7+down},{46,11+down},{46,44+down},{42,48+down},{15,48+down},{11,44+down}},C.leather)
  frame(im,15,11+down,27,33,state=="hover" and C.brass3 or C.brass0,2)
  bolt(im,11,9+down); bolt(im,45,9+down); bolt(im,11,47+down); bolt(im,45,47+down)
  local accent=(kind=="previous") and C.cyan or C.red
  if state=="disabled" then accent=Color{r=75,g=75,b=68,a=255} end
  if kind=="previous" then
    poly(im,{{17,28+down},{30,16+down},{30,23+down},{40,23+down},{40,33+down},{30,33+down},{30,40+down}},accent)
  elseif kind=="next" then
    poly(im,{{39,28+down},{26,16+down},{26,23+down},{16,23+down},{16,33+down},{26,33+down},{26,40+down}},accent)
  else
    for i=0,16 do
      fill(im,18+i,17+down+i,4,4,accent)
      fill(im,34-i,17+down+i,4,4,accent)
    end
  end
  if state=="hover" then h(im,17,8+down,23,C.brass3,2) end
  return im
end

local function indicator()
  local im=image(132,38)
  fill(im,4,5,124,30,C.shadow)
  poly(im,{{3,9},{9,3},{123,3},{129,9},{129,29},{123,35},{9,35},{3,29}},C.ink)
  poly(im,{{6,10},{11,6},{121,6},{126,10},{126,28},{121,32},{11,32},{6,28}},C.brass0)
  fill(im,10,9,112,20,C.leather)
  h(im,15,11,102,C.brass1,2); h(im,15,27,102,C.ink,2)
  return im
end

local function drawDots(im,active,ox,oy)
  for i=0,3 do
    local x=ox+i*24
    if i==active then
      poly(im,{{x,oy-6},{x+6,oy},{x,oy+6},{x-6,oy}},C.brass3)
      disc(im,x,oy,2,C.brass1)
    else
      disc(im,x,oy,4,C.ink); disc(im,x,oy,3,C.brass0)
    end
  end
end

local rail=image(960,104)
fill(rail,0,0,960,104,C.rail)
h(rail,0,0,960,C.ink,5); h(rail,0,5,960,C.brass0,2)
h(rail,28,15,904,C.brass1,2); h(rail,28,90,904,C.brass0,2)
for x=28,932,32 do fill(rail,x,12,3,3,C.brass0) end
h(rail,330,52,300,C.brass0,2); fill(rail,328,49,5,8,C.brass1); fill(rail,628,49,5,8,C.brass1)
v(rail,817,20,66,C.brass0,2); bolt(rail,818,16); bolt(rail,818,88)
rail:saveAs(runtimeRoot.."/guide_nav_rail_960x104_0_6_0.png")

local plate=indicator(); plate:saveAs(runtimeRoot.."/guide_page_indicator_plate_132x38_0_6_0.png")
local kinds={"previous","next","close"}; local states={"idle","hover","pressed","disabled"}
local generated={}
for _,kind in ipairs(kinds) do
  generated[kind]={}
  for _,state in ipairs(states) do
    local b=button(kind,state); generated[kind][state]=b
    b:saveAs(runtimeRoot.."/guide_nav_"..kind.."_"..state.."_56x58_0_6_0.png")
  end
end

local base=assert(app.open(basePath),"cannot open "..basePath)
local preview=base.cels[1].image:clone(); base:close()
preview:drawImage(rail,Point(0,436))
local prevX,indicatorX,nextX,closeX=352,414,552,850
preview:drawImage(generated.previous.idle,Point(prevX,451))
preview:drawImage(plate,Point(indicatorX,461)); drawDots(preview,1,indicatorX+30,480)
preview:drawImage(generated.next.hover,Point(nextX,451))
preview:drawImage(generated.close.idle,Point(closeX,451))
preview:saveAs(previewRoot.."/guide_nav_compact_layout_preview_960x540_0_6_0.png")

local sheet=image(960,300); fill(sheet,0,0,960,300,Color{r=10,g=8,b=6,a=255})
for ki,kind in ipairs(kinds) do
  for si,state in ipairs(states) do sheet:drawImage(generated[kind][state],Point(90+(ki-1)*300,20+(si-1)*68)) end
end
sheet:saveAs(previewRoot.."/guide_nav_button_states_960x300_0_6_0.png")

local src=Sprite(960,104,ColorMode.RGB); src.layers[1].name="guide_nav_compact_rail"
src.cels[1].image:drawImage(rail,Point(0,0)); src:saveAs(sourceRoot.."/guide_nav_rail_960x104_0_6_0.aseprite"); src:close()
local buttons=Sprite(56,58,ColorMode.RGB); buttons.layers[1].name="previous_next_close_states"
buttons.cels[1].image:drawImage(generated.previous.idle,Point(0,0))
for _,kind in ipairs(kinds) do
  for _,state in ipairs(states) do
    if not (kind=="previous" and state=="idle") then
      local f=buttons:newEmptyFrame(); buttons:newCel(buttons.layers[1],f,generated[kind][state],Point(0,0))
    end
  end
end
buttons:saveAs(sourceRoot.."/guide_nav_button_states_56x58_0_6_0.aseprite"); buttons:close()
