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

-- configures the AI difficulty to function better on drift tracks
function SetupAiSettings(ship)
	ship.Ai.Config.RacingLineSkill = ship.Ai.Config.RacingLineSkill * 1.07
	ship.Ai.Config.GuideForceSlowdown = ship.Ai.Config.GuideForceSlowdown * 0.8
	ship.Ai.Config.AirbrakeSkill = ship.Ai.Config.AirbrakeSkill * 1.5
end