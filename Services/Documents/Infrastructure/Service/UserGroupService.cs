

using Application.Services;
using Domain.Ports;
using Newtonsoft.Json;
using Serilog;

namespace Infrastructure.Service
{
    public class UserGroupService(IUserGroupPort _userGroupPort): IUserGroupService
    {
       
        public async Task<Guid> GetGroupForUser(string owner)
        {
            try
            {
                var res = await _userGroupPort.GetFirstGroupForUser(owner);
                var responseContent = await res.Content.ReadAsStringAsync();
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Log.Error($"Error Message : {responseContent}");
                    throw new HttpRequestException("An error has occured, please try again later");
                }
                var JSONObj = JsonConvert.DeserializeObject<Guid>(responseContent)!;
                return JSONObj;
            }

            catch (HttpRequestException ex)
            {
                Log.Error(ex.ToString());
                throw new HttpRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new Exception($"Exception: {ex.Message}");
            }

        }
        public async Task<List<string>> GetGroupId(string owner)
        {
            try
            {
                var res = await _userGroupPort.GetGroupsId(owner);
                var responseContent = await res.Content.ReadAsStringAsync();
                if (res.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Log.Error($"Error Message : {responseContent}");
                    throw new HttpRequestException("An error has occured, please try again later");
                }
                var JSONObj = JsonConvert.DeserializeObject<List<string>>(responseContent)!;
                return JSONObj;
            }

            catch (HttpRequestException ex)
            {
                Log.Error(ex.ToString());
                throw new HttpRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw new Exception($"Exception: {ex.Message}");
            }

        }
    }
}
