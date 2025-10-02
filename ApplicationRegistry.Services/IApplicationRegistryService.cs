using ApplicationRegistry.Model;

namespace ApplicationRegistry.Services
{
    public interface IApplicationRegistryService
    {
        Task<ApplicationRegistryHierarchyForApplicationInstanceResult> GetApplicationRegistryHierarchyInfo(int applicationInstanceID);
        Task<ApplicationInstanceLocationResponse> GetInstanceLocation(int applicationInstanceID);
        Task IncrementRegistryInstanceHeartbeat(int applicationInstanceID);
        Task<ApplicationActiveResult> IsActive(ApplicationActiveRequest applicationActiveRequest);
        Task<ApplicationRegistryResult> Register(ApplicationRegistryEntry applicationRegistryEntry);
        Task<ApplicationRegistryResult> RegisterSelf();
        Task SetApplicationActiveFlag(InstanceIsActive isActiveInstance);
        Task<VerificationStatusTypeEnum> VerifyApplicationModel(VerifyApplicationModel verifyApplicationModel);
        Task<WhoIsResponse> WhoIs(int applicationInstanceID);
    }
}