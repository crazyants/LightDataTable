﻿namespace Generic.LightDataTable.Helper
{
    public enum ItemState
    {
        Added, // is the default
        Changed // this is mostly used in IDbRuleTrigger AfterSave incase we want to reupdate the record
    }

    public enum RoundingConvention
    {
        None,
        Normal,
        RoundUpp,
        RoundDown
    }
}
