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


        /// <summary> 
        /// Retrieves all MedicalAidDependent records from the database. 
        /// </summary>
        /// <returns>A list of MedicalAidDependent entities.</returns>
        public async Task<List<MedicalAidDependent>> GetAllMedicalAidDependentsAsync()
        {
            return await _context.MedicalAidDependents
                    .ToListAsync();

        }
        /// <summary> 
        /// Retrieves a Medical Aid dependent by their dependent ID. 
        /// </summary> 
        /// <param name="dependentId">The dependent ID.</param> 
        /// <returns> /// The MedicalAidDependent entity if found, otherwise null.</returns>
        public async Task<MedicalAidDependent> GetMedicalAidDependentByIdAsync(string dependentId)
        {
            return await _context.MedicalAidDependents
              .FirstOrDefaultAsync(d => d.DependentId == dependentId);
        }
        /// <summary> 
        /// Creates a new Medical Aid dependent in the database. 
        /// </summary> 
        /// <param name="medicalAidDependentModel">The Medical Aid dependent model to be added.</param>
        /// <returns>The created MedicalAidDependent entity.</returns>
        public async Task<MedicalAidDependent> CreateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel)
        {
            await _context.MedicalAidDependents.AddAsync(medicalAidDependentModel);
            await _context.SaveChangesAsync();
            return medicalAidDependentModel;
        }
        /// <summary>Retrieves all active child dependents from the database.</summary>
        /// <returns>A list of active MedicalAidDependent entities with a Child relationship.</returns>
        public async Task<List<MedicalAidDependent>> GetActiveChildDependentsAsync()
        {
            return await _context.MedicalAidDependents
                .Where(d =>
                    d.IsActive &&
                    d.Relationship == Relationship.Child)
                .ToListAsync();
        }
        /// <summary>Retrieves all Medical Aid dependents associated with a specific employee.</summary> /// <param name="employeeId">The employee ID.</param>
        /// <returns>A list of MedicalAidDependent entities associated with the employee.</returns>
        public async Task<List<MedicalAidDependent>> GetMedicalAidDependentsByEmployeeIdAsync(string employeeId)
        {
            return await _context.MedicalAidDependents
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync();

        }
        /// <summary>Updates an existing Medical Aid dependent in the database.</summary>
        /// <param name="medicalAidDependentModel">The Medical Aid dependent model to be updated.</param>
        /// <returns>The updated MedicalAidDependent entity.</returns>
        public async Task<MedicalAidDependent?> UpdateMedicalAidDependentAsync(MedicalAidDependent medicalAidDependentModel)
        {
            _context.MedicalAidDependents.Update(medicalAidDependentModel);
            await _context.SaveChangesAsync();
            return medicalAidDependentModel;
        }
        /// <summary>Deletes a Medical Aid dependent from the database.</summary>
        /// <param name="dependentId">The dependent ID.</param> 
        /// <returns>True if the dependent was successfully deleted, otherwise false.</returns>
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