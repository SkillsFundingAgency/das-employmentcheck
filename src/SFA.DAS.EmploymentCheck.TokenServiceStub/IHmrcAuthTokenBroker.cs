using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    /// <summary>
    ///     Provides access to an automatically refreshed token. The returned value should 
    ///     be considered short lived and re-fetched when needed - it should not be cached
    ///     locally.
    /// </summary>
    public interface IHmrcAuthTokenBroker
    {
        /// <summary>
        ///     Returns a task that returns the current oauth token.
        /// </summary>
        /// <remarks>
        ///     The task will normally be a completed task, which .net will use
        ///     to optimise by avoiding the unncessary thread switch when used
        ///     in async/await scenarios. 
        /// </remarks>
        /// <returns></returns>
        Task<OAuthAccessToken> GetTokenAsync();
    }
}
