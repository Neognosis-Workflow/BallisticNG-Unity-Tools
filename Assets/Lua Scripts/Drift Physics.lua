-- all ships are setup on countdown start
function OnCountdownStart()
	-- run through all ships
	ships =  Ships.Loaded
	shipCount = ships.Count

	for i=0, shipCount - 1 do
		SetupShipGrip(ships[i])
		SetupAiSettings(ships[i])
	end
end

-- configures a ships grip to use the drift ships grip stat
function SetupShipGrip(ship)

	if Cheats.ModernPhysics == true then
		if Race.Speedclass == ESpeedClass:Toxic then -- toxic
			ship.Settings.AG_GRIP = 5.2
		elseif Race.Speedclass == ESpeedClass:Apex then -- apex
			ship.Settings.AG_GRIP = 5.465
		elseif Race.Speedclass == ESpeedClass:Halberd then -- halberd
			ship.Settings.AG_GRIP = 5.75
		elseif Race.Speedclass == ESpeedClass:Spectre then -- spectre
			ship.Settings.AG_GRIP = 6.5
		elseif Race.Speedclass == ESpeedClass:Zen then -- zen
			ship.Settings.AG_GRIP = 7.25
		end
	else
		if Race.Speedclass == ESpeedClass:Toxic then -- toxic
			ship.Settings.AG_GRIP = 1.6
		elseif Race.Speedclass == ESpeedClass:Apex then -- apex
			ship.Settings.AG_GRIP = 1.8
		elseif Race.Speedclass == ESpeedClass:Halberd then -- halberd
			ship.Settings.AG_GRIP = 2
		elseif Race.Speedclass == ESpeedClass:Spectre then -- spectre
			ship.Settings.AG_GRIP = 2
		elseif Race.Speedclass == ESpeedClass:Zen then -- zen
			ship.Settings.AG_GRIP = 2
		end
	end

	-- 2280 stats
	ship.Settings.MODERN_GRIP_AIR = 3
	ship.Settings.MODERN_AIRBRAKE_SLIDE = 100
	ship.Settings.MODERN_AIRBRAKE_TURN = 5
	ship.Settings.MODERN_AIRBRAKE_DRAG = 2
	ship.Settings.MODERN_AIRBRAKE_GAIN = 500
	ship.Settings.MODERN_AIRBRAKE_FALLOFF = 500

	-- airbrake grip is reduced based on the speed class
	ship.Settings.MODERN_AIRBRAKE_GRIP = 4.75
	if Race.Speedclass == ESpeedClass:Toxic then
		ship.Settings.MODERN_AIRBRAKE_GRIP = ship.Settings.MODERN_AIRBRAKE_GRIP * 0.87
	elseif Race.Speedclass == ESpeedClass:Apex then
		ship.Settings.MODERN_AIRBRAKE_GRIP = ship.Settings.MODERN_AIRBRAKE_GRIP * 0.92
	end
end

-- configures the AI difficulty to function better on drift tracks
function SetupAiSettings(ship)
	ship.Ai.Config.RacingLineSkill = ship.Ai.Config.RacingLineSkill * 1.07
	ship.Ai.Config.GuideForceSlowdown = ship.Ai.Config.GuideForceSlowdown * 0.8
	ship.Ai.Config.AirbrakeSkill = ship.Ai.Config.AirbrakeSkill * 1.5
end