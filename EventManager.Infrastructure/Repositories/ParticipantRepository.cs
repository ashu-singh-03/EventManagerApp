using Dapper;
using EventManager.Application.Interfaces;
using EventManager.Domain.Entities;
using EventManager.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EventManager.Infrastructure.Repositories
{
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly DapperContext _context;

        public ParticipantRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Participant>> GetParticipantsByEventAsync(int eventId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<Participant>(
                "sp_GetParticipantsByEvent",
                new { p_EventId = eventId }, // must match SP parameter exactly
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }


        public async Task<Participant> GetParticipantByIdAsync(int participantId)
        {
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<Participant>(
                "sp_GetParticipantById",
                new { p_ParticipantId = participantId }, // Must match SP parameter exactly
                commandType: System.Data.CommandType.StoredProcedure);
            return result;
        }


        public async Task SaveParticipantAsync(Participant participant)
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                p_ParticipantId = participant.ParticipantId,
                p_EventId = participant.EventId,
                p_FirstName = participant.FirstName,
                p_LastName = participant.LastName,
                p_Email = participant.Email,
                p_Phone = participant.Phone,
                p_Company = participant.Company,         
                p_Department = participant.Department,  
                p_Notes = participant.Notes,
                p_QrCodeHash = participant.QrCodeHash,
                p_CreatedBy = participant.CreatedBy,
                p_UpdatedBy = participant.UpdatedBy
            };

            await connection.ExecuteAsync(
                "sp_SaveParticipant",
                parameters,
                commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task DeleteParticipantAsync(int participantId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "sp_DeleteParticipant",
                new { p_ParticipantId = participantId }, // must match SP parameter name
                commandType: System.Data.CommandType.StoredProcedure);
        }
        public async Task DeleteTempParticipantsAsync(int eventId, string createdBy)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM participants_temp WHERE event_id = @eventId AND created_by = @createdBy",
                new { eventId, createdBy }
            );
        }

        public async Task BulkInsertToTempTableAsync(DataTable data)
        {
            using var connection = _context.CreateConnection() as MySqlConnection;
            if (connection != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    await connection.ExecuteAsync(
                        "usp_InsertParticipantTemp",
                        new
                        {
                            p_first_name = row["first_name"],
                            p_last_name = row["last_name"],
                            p_email = row["email"],
                            p_phone = row["phone"],
                            p_company = row["company"],
                            p_department = row["department"],
                            p_notes = row["notes"],
                            p_event_id = row["event_id"],
                            p_created_by = row["created_by"],
                            p_error_message = row["error_message"] ?? DBNull.Value // Use the column we added
                        },
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
        }
        public async Task<DataTable> ValidateTempParticipantsAsync(int eventId, string createdBy)
        {
            using var connection = _context.CreateConnection() as MySqlConnection;
            if (connection != null)
            {
                var cmd = new MySqlCommand("sp_ValidateTempParticipants", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_event_id", eventId);
                cmd.Parameters.AddWithValue("@p_created_by", createdBy);

                var da = new MySqlDataAdapter(cmd);
                var dt = new DataTable();

                await connection.OpenAsync();
                da.Fill(dt);
                connection.Close();

                return dt;
            }
            return new DataTable();
        }

        public async Task<int> ImportTempToMainAsync(int eventId, string createdBy)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@p_event_id", eventId);
            parameters.Add("@p_created_by", createdBy);
            parameters.Add("@p_rows_imported", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync(
                "sp_ImportTempToMain",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<int>("@p_rows_imported");
        }
    }
}
