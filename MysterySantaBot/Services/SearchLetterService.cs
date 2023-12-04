using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MysterySantaBot.Database;
using MysterySantaBot.Database.Entities;

namespace MysterySantaBot.Services;

public class SearchLetterService
{
    private readonly SantaBotDbContext _db;

    public SearchLetterService(SantaBotDbContext db)
    {
        _db = db;
    }

    public async Task<UserForm?> GetNextUserTelegramIdInSearch(UserForm searcher)
    {
        string sqlStr = @"
							with my_chosen as
							(
								select wuc.chosen_user_telegram_id 
								from app.user_choice wuc 
								where wuc.user_telegram_id = {0}
							)
							select  
								uf.*
							from
								app.user_form uf
							left join app.show_history sh on
								sh.user_telegram_id = {0}
								and sh.shown_user_telegram_id = uf.user_telegram_id 
							where 
								uf.user_telegram_id != {0}
								and uf.user_telegram_id not in (select * from my_chosen)
								and uf.is_hidden != true
								and	uf.description is not null
							order by
								coalesce(sh.last_shown,
								'1970-12-12')
							limit 1
";

        string sql = string.Format(sqlStr, searcher.UserTelegramId);
        
        UserForm? result = _db.UsersForm.FromSqlRaw(sql).ToList().FirstOrDefault();
        return result;
    }
}