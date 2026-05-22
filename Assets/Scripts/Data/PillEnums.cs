namespace PharmacySim.Data
{
    public enum PillShape
    {
        RoundTablet,
        OvalTablet,
        Capsule,
        HexagonalTablet,
        SmallBead,
        LargeCoatedTablet
    }

    public enum ControlledStatus
    {
        NonControlled,
        ScheduleIV,
        ScheduleIII,
        ScheduleII
    }

    public enum SieveFilterType
    {
        CircularSmall,
        CircularMedium,
        CircularLarge,
        CapsuleSlot,
        GridFine,
        GridCoarse,
        HexagonalSlot
    }

    public enum WorkflowStep
    {
        ReceiveOrder,
        ReadPrescription,
        VerifyMedication,
        SortPills,
        IdentifyImprint,
        DispenseQuantity,
        FillBottle,
        PrintLabel,
        ApplyLabel,
        FinalVerification,
        SubmitOrder
    }

    public enum PrescriptionUrgency
    {
        Routine,
        Priority,
        Stat
    }
}
