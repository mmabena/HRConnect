namespace HRConnect.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Threading.Tasks;
    public class MedicalAidDependentRepository : IMedicalAidDependentRepository
    {
        private readonly ApplicationDBContext _context;

        public MedicalAidDependentRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<MedicalAidDependent>> GetAllMedicalAidDependentsAsync()
        {
            return await _context.MedicalAidDependents
                    .ToListAsync();

        }
        public async Task<MedicalAidDependent> GetMedicalAidDependentByIdAsync(string dependentId)
        {
            return await _context.MedicalAidDependents
              .FirstOrDefaultAsync(d => d.DependentId == dependentId);
        }

        public async Task<MedicalAidDependent> CreateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel)
        {
            await _context.MedicalAidDependents.AddAsync(medicalAidDependentModel);
            await _context.SaveChangesAsync();
            return medicalAidDependentModel;

        }

        public async Task<List<MedicalAidDependent>> GetMedicalAidDependentsByEmployeeIdAsync(string employeeId)
        {
            return await _context.MedicalAidDependents
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();

        }

        public async Task<MedicalAidDependent?> UpdateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel)
        {
            _context.MedicalAidDependents.Update(medicalAidDependentModel);
            await _context.SaveChangesAsync();
            return medicalAidDependentModel;

        }

        public async Task<bool> DeleteMedicalAidDependentAsync(string dependentId)
        {
            var existingDependent = await _context.MedicalAidDependents
                    .FirstOrDefaultAsync(d => d.DependentId == dependentId);

            if (existingDependent == null)
                return false;

            _context.MedicalAidDependents.Remove(existingDependent);
            await _context.SaveChangesAsync();
            return true;

        }

    }
}