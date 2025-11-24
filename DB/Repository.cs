using coach_search.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace coach_search.DB
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly Context _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(Context context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id) ?? null;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();//.AsNoTracking()
        }

        public virtual async Task AddAsync(T entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public class UserRepository : Repository<User>
    {
        public UserRepository(Context context) : base(context) { }

        public override async Task AddAsync(User entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            if(entity.Role == 1)
            {
                await this._context.TutorInfos.AddAsync(new TutorInfo() { Description = string.Empty, UserId = entity.Id});
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetTutorsAsync()
        {
            return await _dbSet
                .Where(u => u.Role == 1)
                .Include(u => u.TutorInfo)
                .ToListAsync();
        }

        public async Task<List<User>> GetClientsAsync()
        {
            return await _dbSet.Where(u => u.Role == 0).ToListAsync();
        }
    }

    public class TutorRepository : Repository<TutorInfo>//IRepository<TutorInfo>
    {
        public TutorRepository(Context context) : base(context) { }

        public async Task<TutorInfo?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
    }


    public class ReviewRepository : Repository<Review>
    {
        public ReviewRepository(Context context) : base(context) { }

        public async Task<List<Review>> GetReviewsForTutorAsync(int tutorId)
        {
            return await _dbSet
                .Where(r => r.TutorId == tutorId)
                .Include(r => r.Author)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(int authorId)
        {
            return await _dbSet.Where(r => r.AuthorId == authorId).ToListAsync();   
        }
    }

    public class AppointmentRepository : Repository<Appointment>
    {
        public AppointmentRepository(Context context) : base(context) { }

        public override async Task AddAsync(Appointment entity)
        {
            MessageBox.Show(entity.Status.ToString());
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Appointment>> GetTutorAppointmentsAsync(int tutorId)
        {
            return await _dbSet
                .Where(a => a.TutorId == tutorId)
                .Include(a => a.Student)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetStudentAppointmentsAsync(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId)
                .Include(a => a.Tutor)
                .ToListAsync();
        }
    }
}
