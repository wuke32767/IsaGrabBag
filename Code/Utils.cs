namespace Celeste.Mod.IsaGrabBag {
    public static class Utils {
        public static float Mod(float x, float m) {
            return ((x % m) + m) % m;
        }

        public static EntityData GetEntityData(this MapData mapData, string entityName) {
            foreach (LevelData levelData in mapData.Levels) {
                if (levelData.GetEntityData(entityName) is EntityData entityData) {
                    return entityData;
                }
            }

            return null;
        }

        public static bool HasEntity(this MapData mapData, string entityName) {
            return mapData.GetEntityData(entityName) != null;
        }

        public static EntityData GetEntityData(this LevelData levelData, string entityName) {
            foreach (EntityData entity in levelData.Entities) {
                if (entity.Name == entityName) {
                    return entity;
                }
            }

            return null;
        }
    }
}
