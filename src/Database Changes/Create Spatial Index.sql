drop index if exists idx_lsd_geometry;

CREATE INDEX idx_lsd_geometry
ON Public."LegalSubDivision"
USING GIST ("Geometry");

drop index if exists idx_parcel_geometry;

CREATE INDEX idx_parcel_geometry
ON Public."Parcel"
USING GIST ("Geometry");

drop index if exists idx_farm_geometry;

CREATE INDEX idx_farm_geometry
ON Public."Farm"
USING GIST ("Geometry");

drop index if exists idx_reach_geometry;

CREATE INDEX idx_reach_geometry
ON Public."Reach"
USING GIST ("Geometry");

drop index if exists idx_watershed_geometry;

CREATE INDEX idx_watershed_geometry
ON Public."Watershed"
USING GIST ("Geometry");

drop index if exists idx_subwatershed_geometry;

CREATE INDEX idx_subwatershed_geometry
ON Public."SubWatershed"
USING GIST ("Geometry");

drop index if exists idx_municipality_geometry;

CREATE INDEX idx_municipality_geometry
ON Public."Municipality"
USING GIST ("Geometry");