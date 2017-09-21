namespace Generic.LightDataTable.InterFace
{
    public interface IDbRuleTrigger 
    {
        void BeforeSave(ICustomRepository repository, IDbEntity itemDbEntity);

        void AfterSave(ICustomRepository repository, IDbEntity itemDbEntity, long objectId);
    }
}
