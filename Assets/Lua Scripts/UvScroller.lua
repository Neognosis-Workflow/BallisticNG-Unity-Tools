-- var scrollAmount Vector2
-- var renderer MeshRenderer
-- var matIndex Integer
-- var duplicateMat Boolean

-- check variables
canRun = true
if renderer == nil then
    print("<color=red>" .. scriptName.. " on " .. gameObject.name .. " script can't run (No Mesh Renderer)</color>")
    canRun = false
end

if scrollAmount == nil then
    scrollAmount = Vector2.right
end

if matIndex == nil then
    matIndex = 1
end

if duplicateMat == nil then
    duplicateMat = true
end

-- init data
scrollPos = Vector2.zero

-- get material
if canRun then
    
    -- calling materials will create instances of the materials. sharedMaterials references the materials instead
    if duplicateMat then materials = renderer.materials else materials = renderer.sharedMaterials end

    matLen = #materials
    if matIndex < 0 or matIndex > matLen - 1 then
        print("<color=red>" .. scriptName.. " on " .. gameObject.name .. " script can't run (Invalid Material Index)</color>")
        canRun = false
    else
        material = materials[matIndex]
    end
end

-- update material with scrolled position
function Update(unscaledDeltaTime, deltaTime)
    if canRun then
        scrollPos = scrollPos + scrollAmount * deltaTime
        material.SetTextureOffset("_MainTex", scrollPos)
    end
end
