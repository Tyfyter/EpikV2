namespace EpikV2 {
    public interface IDrawAfterNPCs {
        void DrawPostNPCLayer();
    }
    public static class IDrawAfterNPCsExt {
        public static bool AddToAfterNPCQueue(this IDrawAfterNPCs entity){
            if (EpikV2.drawAfterNPCs is null) return false;
            EpikV2.drawAfterNPCs.Add(entity);
            return true;
        }
    }
}