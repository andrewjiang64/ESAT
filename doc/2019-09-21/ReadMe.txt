2019-09-20 By Shawn 

This folder contains the input files and output database for ESAT test package.

All input files are stored in the \Input folder, within this folder

	•	\Vectors folder contains 11 vectors (ESRI shapefile format): Watershed, Municipality, SubArea, Reach, Subbasin, SubWatershed, LegalSubDivision, Parcel, Feedlot, IsolatedWetland, and CatchBasin	
	
	•	\Results folder contains 4 test database results (CSV format): ScenarioResultsSubArea, ScenarioResultsReach, BmpEffectivenessSubArea, BmpEffectivenessReach. *** Please note, -999 is NO DATA value, so this result will not be imported into ESAT. ***
	

After importing, 

	•	Vectors are stored in their spatial table linked with ModelComponent table.
	
	•	Scenario results are stored in the ScenarioModelResult table.
	
	•	BMP effectiveness are stored in the UnitScenario and UnitScenarioEffectivenss table.

