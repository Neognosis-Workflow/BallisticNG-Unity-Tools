StatReference = nil
SpawnedShips = 0

function OnShipSpawned(ship)
	-- If we haven't loaded the barracuda model c to use as our stat reference then load it
	--
	-- The ship is loaded as an unlocked bundle, which means we'll be able to unload it once we're done so its assets don't
	-- stay in memory for the remainder of the race if a real ship in the race isn't using it.
	--
	-- For reference in your own scripts, you can also call LoadShipPrefab with a ship display name string. This will let you 
	-- load custom ships if they're installed.
	if StatReference == nil then
		StatReference = Api:LoadShipPrefab(EShips:BarracudaModelC, 0)
		ShipSettings:SetupStatModifiers(StatReference.settings) -- this applies stat modifiers based on the current game config
	end

	-- apply the custom settings
	SetupShipGrip(ship.Settings, StatReference.settings)
	if Cheats:ModernPhysics ~= true then
		SetupAiSettings(ship)
	else
		SetupAiSettingsModernPhysics(ship)
	end

	--- once all ships have been spawned, destroy the loaded model c and unload any unlocked ship asset bundles
	SpawnedShips = SpawnedShips + 1
	if SpawnedShips >= Ships.Loaded.Count then
		Object:Destroy(StatReference.settings.gameObject)
		Api:UnloadShipBundles()
	end

end

-- configures a ships grip to use the drift ships grip stat
function SetupShipGrip(settings, modelC)
	settings.AG_GRIP = modelC.AG_GRIP
	settings.MODERN_GRIP_AIR = modelC.MODERN_GRIP_AIR
	settings.MODERN_AIRBRAKE_SPRING_GAIN = modelC.MODERN_AIRBRAKE_SPRING_GAIN 
	settings.MODERN_AIRBRAKE_SPRING_FALLOFF = modelC.MODERN_AIRBRAKE_SPRING_FALLOFF
	settings.MODERN_AIRBRAKE_GRIP_SPRING_GAIN = modelC.MODERN_AIRBRAKE_GRIP_SPRING_GAIN
	settings.MODERN_AIRBRAKE_GRIP_SPRING_FALLOFF = modelC.MODERN_AIRBRAKE_GRIP_SPRING_FALLOFF
	settings.MODERN_AIRBRAKE_SLIDE = modelC.MODERN_AIRBRAKE_SLIDE
	settings.MODERN_AIRBRAKE_TURN = modelC.MODERN_AIRBRAKE_TURN
	settings.MODERN_AIRBRAKE_DRAG = modelC.MODERN_AIRBRAKE_DRAG
	settings.MODERN_AIRBRAKE_GAIN = modelC.MODERN_AIRBRAKE_GAIN
	settings.MODERN_AIRBRAKE_FALLOFF = modelC.MODERN_AIRBRAKE_FALLOFF
	settings.MODERN_AIRBRAKE_GRIP = modelC.MODERN_AIRBRAKE_GRIP
end

-- configures the AI difficulty to function better on drift tracks with 2159 and floorhugger physics
function SetupAiSettings(ship)
	ship.Ai.Config.RacingLineSkill = ship.Ai.Config.RacingLineSkill * 1.7181860215053763440860215053763
	ship.Ai.Config.GuideForceSlowdown = ship.Ai.Config.GuideForceSlowdown * 0.16777215
	ship.Ai.Config.AirbrakeSkill = ship.Ai.Config.AirbrakeSkill * 25.628908333333333333333333333333
end

-- configures the AI difficulty to function better on drift tracks with 2280 physics
function SetupAiSettingsModernPhysics(ship)
	ship.Ai.Config.GuideForceSlowdown = ship.Ai.Config.GuideForceSlowdown * 0.16777215
	ship.Ai.Config.AirbrakeSkill = ship.Ai.Config.AirbrakeSkill * 1.2
end