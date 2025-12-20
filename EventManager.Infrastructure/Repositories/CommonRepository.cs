using Dapper;
using EventManager.Application.DTOs;
using EventManager.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class CommonRepository
{

    private readonly DapperContext _context;

    public CommonRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<List<DropdownItemDto>> GetDropdownAsync(string flag, int? eventId = null)
    {
        using var connection = _context.CreateConnection();

        var result = await connection.QueryAsync<DropdownItemDto>(
            "usp_DropdownList",          
            new { p_flag = flag, p_eventId = eventId },  
            commandType: CommandType.StoredProcedure);

        return result.AsList();
    }
}
