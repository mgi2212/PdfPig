namespace IccProfileNet
{
    internal static class IccTags
    {
        // v4.4 and v2.4 tags
        internal const string DeviceModelDescTag = "dmdd";
        internal const string DeviceMfgDescTag = "dmnd";
        internal const string GamutTag = "gamt";
        internal const string GreenTRCTag = "gTRC";
        internal const string GrayTRCTag = "kTRC";
        internal const string LuminanceTag = "lumi";
        internal const string MeasurementTag = "meas";
        internal const string NamedColor2Tag = "ncl2";
        internal const string Preview0Tag = "pre0";
        internal const string Preview1Tag = "pre1";
        internal const string Preview2Tag = "pre2";
        internal const string ProfileSequenceDescTag = "pseq";
        internal const string OutputResponseTag = "resp";
        internal const string RedTRCTag = "rTRC";
        internal const string CharTargetTag = "targ";
        internal const string TechnologyTag = "tech";
        internal const string ViewingConditionsTag = "view";
        internal const string ViewingCondDescTag = "vued";
        internal const string MediaWhitePointTag = "wtpt";
        internal const string AToB0Tag = "A2B0";
        internal const string AToB1Tag = "A2B1";
        internal const string AToB2Tag = "A2B2";
        internal const string BToA0Tag = "B2A0";
        internal const string BToA1Tag = "B2A1";
        internal const string BToA2Tag = "B2A2";
        internal const string BlueTRCTag = "bTRC";
        internal const string CalibrationDateTimeTag = "calt";
        internal const string ChromaticAdaptationTag = "chad";
        internal const string ChromaticityTag = "chrm";
        internal const string CopyrightTag = "cprt";
        internal const string ProfileDescriptionTag = "desc";

        // v4.4 and v2.4 tags with different names
        // v4.4 names
        internal const string BlueMatrixColumnTag = "bXYZ";
        internal const string GreenMatrixColumnTag = "gXYZ";
        internal const string RedMatrixColumnTag = "rXYZ";

        // v2.4 names
        internal const string BlueColorantTag = "bXYZ";
        internal const string GreenColorantTag = "gXYZ";
        internal const string RedColorantTag = "rXYZ";

        // v4.4 only tags
        internal const string MetadataTag = "meta";
        internal const string ProfileSequenceIdentifierTag = "psid";
        internal const string PerceptualRenderingIntentGamutTag = "rig0";
        internal const string SaturationRenderingIntentGamutTag = "rig2";
        internal const string BToD0Tag = "B2D0";
        internal const string BToD1Tag = "B2D1";
        internal const string BToD2Tag = "B2D2";
        internal const string BToD3Tag = "B2D3";
        internal const string CicpTag = "cicp";
        internal const string ColorimetricIntentImageStateTag = "ciis";
        internal const string ColorantTableOutTag = "clot";
        internal const string ColorantOrderTag = "clro";
        internal const string ColorantTableTag = "clrt";
        internal const string DToB0Tag = "D2B0";
        internal const string DToB1Tag = "D2B1";
        internal const string DToB2Tag = "D2B2";
        internal const string DToB3Tag = "D2B3";

        // v2.4 only tags
        internal const string NamedColorTag = "ncol";
        internal const string Ps2RenderingIntentTag = "ps2i";
        internal const string Ps2CSATag = "ps2s";
        internal const string Ps2CRD0Tag = "psd0";
        internal const string Ps2CRD1Tag = "psd1";
        internal const string Ps2CRD2Tag = "psd2";
        internal const string Ps2CRD3Tag = "psd3";
        internal const string ScreeningDescTag = "scrd";
        internal const string ScreeningTag = "scrn";
        internal const string UcrbgTag = "bfd ";
        internal const string MediaBlackPointTag = "bkpt";
        internal const string CrdInfoTag = "crdi";
        internal const string DeviceSettingsTag = "devs";
    }
}
