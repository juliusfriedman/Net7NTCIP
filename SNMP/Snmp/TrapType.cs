namespace ASTITransportation.Snmp
{
    public enum TrapTypes : int
    {
        ColdStart = 0,
        WarmStart = 1,
        LinkDown = 2,
        LinkUp = 3,
        AuthenticationFail = 4,
        EGPNeighborLoss = 5,
        EnterpriseSpecific = 6
    }
}
