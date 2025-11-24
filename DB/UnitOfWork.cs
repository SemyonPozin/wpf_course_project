using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coach_search.DB
{
    namespace coach_search.DB
    {
        public class UnitOfWork : IDisposable
        {
            private readonly Context _context;

            // Репозитории
            private UserRepository _userRepository;
            private TutorRepository _tutorRepository;
            private ReviewRepository _reviewRepository;
            private AppointmentRepository _appointmentRepository;

            public UnitOfWork(Context context)
            {
                _context = context;
            }

            // Доступ к репозиториям через свойства
            public UserRepository Users => _userRepository ??= new UserRepository(_context);
            public TutorRepository Tutors => _tutorRepository ??= new TutorRepository(_context);
            public ReviewRepository Reviews => _reviewRepository ??= new ReviewRepository(_context);
            public AppointmentRepository Appointments => _appointmentRepository ??= new AppointmentRepository(_context);

            // Сохраняем все изменения в базе
            public async Task<int> SaveAsync()
            {
                return await _context.SaveChangesAsync();
            }

            // IDisposable
            private bool _disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _context.Dispose();
                    }
                    _disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
