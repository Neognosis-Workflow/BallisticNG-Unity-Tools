-----------------------------------------------------------------------------------------------------------
---- USAGE ------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- This script allows you to attach additional meshes to the ship and have it be lit and toggled in different
--  camera views.

-- Use the Create Variables From Script button in the Lua Runner to be given the following variables:
-- rendererObject       : A reference to the gameobject which holds the renderer. This can be any kind of
--                        unity renderer (MeshRenderer, SkinnedMeshRenderer, TrailRenderer, LineRenderer, etc)
--
-- useShipMaterial      : Whether a material on the renderer will be assigned the ships main material. This can
--                        be used to create additional ship mesh that can respond to ship livery changes.
--
-- shipMaterialIndex    : The index of the material that will be replaced with the ships main material.
-- inExternal           : Whether this mesh will be visible in external camera views.
-- inInternal           : Whether this mesh will be visible in the internal camera view.
-- inCockpit            : Whether this mesh will be visible in the cockpit camera view.

-----------------------------------------------------------------------------------------------------------
---- UNITY VARIABLES --------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-- var rendererObject GameObject
-- var useShipMaterial Boolean
-- var shipMaterialIndex Integer
-- var inExternal Boolean
-- var inInternal Boolean
-- var inCockpit Boolean

----------------------------------------------------------------------------------------------------------

-- if any of the variables haven't been created in Unity, set them to default values.
if (useShipMaterial == nil) then useShipMaterial = false end
if (shipMaterialIndex == nil) then shipMaterialIndex = 0 end
if (inExternal == nil) then inExternal = true end
if (inInternal == nil) then inInternal = false end
if (inCockpit == nil) then inCockpit = false end

-- the viewing state is how the player is currently viewing the ship. We'll default this to nil and then update
--  it per frame to create the events where we update mesh renderer visibility.
prevViewingState = nil

-- setup and cache material references 
canRun = false
if (rendererObject ~= nil) then
	renderer = rendererObject.GetComponent("Renderer")
	if (renderer ~= nil) then canRun = true end
end

if (canRun) then
	if (useShipMaterial and shipMaterialIndex < renderer.sharedMaterials.Length) then
		-- sharedMaterials is a C# property, we need to store the array in a local variable and then reassign 
		--  it after changing it
		tempMatArr = renderer.sharedMaterials
		tempMatArr[shipMaterialIndex] = ship.ShipRenderer.sharedMaterial
		renderer.sharedMaterials = tempMatArr
	else
		shipMaterialIndex = -1
	end

	materials = renderer.sharedMaterials
	numMaterials = materials.Length
end

-- if we're in splitscreen and this ship is a player, then duplicate the mesh renderer and assign layers 
--  for self rendering for player 1 and player 2
renderers = {}
if (Api:IsSplitscreen() and ship.IsPlayer) then
	dupRenderer = Object:Instantiate(renderer.gameObject)

	playerOneT = renderer.transform
	playerTwoT = dupRenderer.transform

	playerTwoT.SetParent(playerOneT.parent, true)
	playerTwoT.localPosition = playerOneT.localPosition
	playerTwoT.localRotation = playerOneT.localRotation
	playerTwoT.localScale = playerOneT.localScale

	playerTwoRenderer = dupRenderer.GetComponent("Renderer")
	renderer.gameObject.layer = LayerIDs:PlayerOneSelfRender
	playerTwoRenderer.gameObject.layer = LayerIDs:PlayerTwoSelfRender

	renderers[1] = renderer
	renderers[2] = playerTwoRenderer
else -- otherwise just use the renderer as is
	renderers[1] = renderer
end

numRenderers = #renderers

-- Iterates through all cached material references and updates them to follow the ships
--  lighting color. We'll skip the material of index shipMaterialIndex, as the game is
--  already updating this internally.
function UpdateMaterials()
	-- update materials with ship color
	for i = 0, numMaterials - 1 do
		if (i ~= shipMaterialIndex) then
			materials[i].SetColor("_Color", ship.Effects.shipColor)
		end
	end
end

-- Checks if the ship viewing state has changed and updates the rendering state of mesh renderers
--  if it has. 
function UpdateRenderers()
	if (prevViewingState ~= ship.ViewingState) then
		prevViewingState = ship.ViewingState

		if (prevViewingState == EShipViewingState:External) then
			SetRenderersState(inExternal)
		elseif (prevViewingState == EShipViewingState:Internal) then 
			SetRenderersState(inInternal)
		elseif (prevViewingState == EShipViewingState:Cockpit) then
			SetRenderersState(inCockpit)
		end
	end
end

-- Sets the state of the cached mesh renderers
function SetRenderersState(state)
	renderers[1].enabled = state -- players renderer
	
	if (numRenderers == 2) then
		renderers[2].enabled = inExternal -- other players renderer
	end
end

-- Called every rendered frame.
function Update(unscaledDeltaTime, deltaTime)
	if (not canRun) then return end

	UpdateMaterials()
	UpdateRenderers()
end