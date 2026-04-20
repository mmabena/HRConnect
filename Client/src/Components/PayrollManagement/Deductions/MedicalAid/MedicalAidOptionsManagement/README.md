# Medical Aid Options Management - Category Grouping Feature

## Overview

This feature enhances the Medical Aid Options Management system by implementing data grouping and transformation functionality. When a user views or edits a medical aid option, the system now automatically loads and displays all options within the same category, providing a comprehensive view of related options.

## Problem Statement

The original implementation had the following limitations:
- Only displayed a single selected option in the view/edit modal
- Data was received from the database in an unstructured JSON format
- No ability to view or compare options within the same category
- Users had to navigate back to the main grid to view other options in the same category
- No runtime data transformation to group options by category

### Example Scenario
With 7 options across 3 categories (e.g., Hospital Plan, Comprehensive Plan, Network Plan), clicking "View | Edit" on any option would only show that single option. Users could not easily see or edit all options within the same category without multiple navigation steps.

## Implementation Summary

### Files Modified

1. **MedicalAidOptionsManagement.jsx**
   - Modified `handleViewRecord` to pass all options in the selected category
   - Added `categories` prop to `MedicalAidOptionViewModal`

2. **MedicalAidOptionViewModal.jsx**
   - Added `useState` for category selection management
   - Implemented data transformation function to group options by category
   - Added category dropdown selector in the modal body
   - Updated table rendering to use grouped data
   - Added `categories` prop to component signature

3. **MedicalAidOptionViewModal.css**
   - Added styling for category selector container
   - Added styling for dropdown element with focus states

## Detailed Solution Breakdown

### 1. Data Transformation Logic

The solution implements a runtime data transformation function `groupedOptionsByCategory` that converts flat JSON data into a grouped structure:

```javascript
const groupedOptionsByCategory = (options, categories) => {
  const grouped = {};

  options.forEach(option => {
    const categoryId = option.medicalOptionCategoryId;
    const category = categories.find(cat => cat.medicalOptionCategoryId === categoryId);
    const categoryName = category?.medicalOptionCategoryName || 'Unknown';

    if (!grouped[categoryId]) {
      grouped[categoryId] = {
        categoryId,
        categoryName,
        options: []
      };
    }

    grouped[categoryId].options.push(option);
  });

  return Object.values(grouped);
};
```

**Key Features:**
- Iterates through all options and groups them by `medicalOptionCategoryId`
- Looks up category names from the categories array
- Creates a structured object with category metadata and associated options
- Returns an array of category groups for easy iteration

### 2. Component Architecture Changes

#### MedicalAidOptionsManagement.jsx

**Before:**
```javascript
const handleViewRecord = (rowData) => {
  setModalData([rowData] || []);  // Single row only
  setIsModalOpen(true);
};
```

**After:**
```javascript
const handleViewRecord = (rowData) => {
  const categoryId = rowData.medicalOptionCategoryId;
  const categoryOptions = medicalOptions.filter(
    opt => opt.medicalOptionCategoryId === categoryId
  );
  
  setModalData(categoryOptions || []);  // All category options
  setIsModalOpen(true);
};
```

**Rationale:** Filtering by category ID ensures all related options are loaded, enabling comprehensive category-based viewing and editing.

#### MedicalAidOptionViewModal.jsx

Added state management for category selection:
```javascript
const [selectedCategoryId, setSelectedCategoryId] = useState(null);
```

Implemented automatic default selection:
```javascript
useEffect(() => {
  if (groupedData.length > 0 && !selectedCategoryId) {
    setSelectedCategoryId(groupedData[0].categoryId);
  }
}, [groupedData, selectedCategoryId]);
```

**Rationale:** Automatically selects the first category when data loads, providing immediate content display without requiring user interaction.

### 3. UI/UX Enhancements

#### Category Selector Dropdown

Added a dropdown component above the table:
```javascript
{groupedData.length > 0 && (
  <div className="category-selector">
    <label htmlFor="category-dropdown">Select Category:</label>
    <select
      id="category-dropdown"
      value={selectedCategoryId || ''}
      onChange={(e) => setSelectedCategoryId(Number(e.target.value))}
      className="category-dropdown"
    >
      {groupedData.map(category => (
        <option key={category.categoryId} value={category.categoryId}>
          {category.categoryName} ({category.options.length} options)
        </option>
      ))}
    </select>
  </div>
)}
```

**Features:**
- Shows category name with option count for context
- Only renders when grouped data is available
- Updates `selectedCategoryId` state on change
- Triggers table re-render with new category's options

#### Table Rendering Update

Changed from rendering `data` directly to rendering filtered `displayData`:
```javascript
const selectedCategory = groupedData.find(cat => cat.categoryId === selectedCategoryId);
const displayData = selectedCategory?.options || [];

// In table body:
{displayData.map((row, rowIndex) => (
  <tr key={row.id ?? rowIndex}>
    {viewColumns.map((col) => (
      <td key={col}>
        {col.render ? col.render(row[col.key], row) : formatCell(row[col.key])}
      </td>
    ))}
  </tr>
))}
```

**Rationale:** Only displays options from the currently selected category, keeping the UI clean and focused.

### 4. Styling Implementation

Added CSS for the category selector:

```css
.category-selector {
  margin-bottom: 20px;
  padding: 15px;
  background: #f5f5f5;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.category-selector label {
  font-weight: 600;
  color: #333;
}

.category-dropdown {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
  min-width: 250px;
  background: white;
  cursor: pointer;
}

.category-dropdown:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 2px rgba(74, 144, 226, 0.2);
}
```

**Design Decisions:**
- Light gray background (#f5f5f5) to visually separate from table
- Flexbox layout for horizontal alignment
- Blue focus state (#4a90e2) for accessibility and visual feedback
- Minimum width (250px) to accommodate longer category names
- Subtle box-shadow on focus for modern UI feel

## Technical Decisions & Rationale

### Why Group Data by Category?

**Reason:** Medical aid options are inherently organized by categories (e.g., Hospital Plan, Comprehensive Plan). Users typically need to view or compare options within the same category rather than across different categories. Grouping at runtime transforms the flat database structure into a more logical, user-centric format.

**Benefits:**
- Aligns with business logic and user mental model
- Enables category-based navigation
- Simplifies data comparison within groups
- Reduces cognitive load by presenting related data together

### Why Use a Dropdown Selector?

**Reason:** A dropdown provides a space-efficient way to switch between categories without cluttering the UI. It allows users to quickly navigate between categories while maintaining focus on the current view.

**Benefits:**
- Minimal screen real estate usage
- Familiar UI pattern for most users
- Clearly shows the currently selected category
- Can display additional context (option count) in the label
- Easy to implement and maintain

### Why Pass All Category Options Instead of Single Row?

**Reason:** The original implementation only passed the selected row, limiting users to viewing/editing one option at a time. By passing all options in the category, users can:
- View all options in context
- Compare contribution amounts across salary brackets
- Edit multiple options in a single session
- Understand the category structure better

**Benefits:**
- Reduces navigation steps
- Provides comprehensive context
- Enables bulk editing scenarios
- Improves workflow efficiency

### Reusability Considerations

The solution is designed with reusability in mind:
- Data transformation function is pure and testable
- Component accepts generic `categories` prop (not hardcoded)
- Styling is modular and can be applied to other dropdowns
- Grouping logic can be extracted to a utility file if needed

## Code Structure

### Helper Functions

#### `groupedOptionsByCategory(options, categories)`
- **Purpose:** Transforms flat option data into grouped structure
- **Parameters:**
  - `options`: Array of medical aid option objects
  - `categories`: Array of category objects for lookup
- **Returns:** Array of category groups with associated options
- **Complexity:** O(n * m) where n = options count, m = categories count

### Component Flow

```
User clicks "View | Edit"
    ↓
handleViewRecord(rowData)
    ↓
Filter medicalOptions by categoryId
    ↓
setModalData(categoryOptions)
    ↓
Modal opens with data prop
    ↓
groupedOptionsByCategory(data, categories)
    ↓
setSelectedCategoryId(first category)
    ↓
Render dropdown + table
    ↓
User changes dropdown selection
    ↓
setSelectedCategoryId(new value)
    ↓
Table re-renders with new category's options
```

### Data Flow

```
Database (Flat JSON)
    ↓
MedicalAidOptionsManagement
    ↓
handleViewRecord filters by category
    ↓
MedicalAidOptionViewModal receives data
    ↓
groupedOptionsByCategory transforms data
    ↓
State: groupedData (array of categories)
    ↓
State: selectedCategoryId (currently selected)
    ↓
displayData (filtered options)
    ↓
Table renders displayData
```

## Usage Guide

### For Developers

#### Passing Data to Modal

```javascript
<MedicalAidOptionViewModal
  isOpen={isModalOpen}
  onClose={handleCloseModal}
  title="Medical Aid Options"
  data={modalData}              // Array of options (filtered by category)
  categories={medicalOptionsCategory}  // Array of all categories
/>
```

#### Data Structure Requirements

**Options Array:**
```javascript
[
  {
    id: 1,
    medicalOptionName: "Basic Hospital",
    medicalOptionCategoryId: 1,
    salaryBracketMin: 0,
    salaryBracketMax: 10000,
    monthlyMSAContributionPrincipal: 500,
    // ... other fields
  }
]
```

**Categories Array:**
```javascript
[
  {
    medicalOptionCategoryId: 1,
    medicalOptionCategoryName: "Hospital Plan"
  }
]
```

### For End Users

1. **View Options:** Click "View | Edit" on any medical aid option in the main grid
2. **Category Selection:** The modal opens showing all options in the same category
3. **Switch Categories:** Use the dropdown to view options from other categories
4. **Edit Data:** Edit fields directly in the table (existing functionality)
5. **Close Modal:** Click the Close button or press Escape

## Future Enhancements

### Potential Improvements

1. **Search/Filter Within Category:** Add search functionality to filter options within the selected category
2. **Bulk Edit Mode:** Enable editing multiple options simultaneously
3. **Category Summary:** Show aggregate statistics (average contribution, total options) per category
4. **Export Functionality:** Allow exporting category data to CSV/PDF
5. **Undo/Redo:** Implement undo/redo for edits within the modal
6. **Validation:** Add real-time validation for contribution amounts
7. **Category Comparison:** Side-by-side comparison view for different categories
8. **Performance Optimization:** Implement virtual scrolling for large datasets

### Extensibility Considerations

- Extract `groupedOptionsByCategory` to a shared utility file
- Create a reusable `CategorySelector` component for other parts of the application
- Implement a context for category management across multiple components
- Add TypeScript interfaces for better type safety
- Create unit tests for the transformation logic

## Troubleshooting

### Common Issues

**Issue:** Categories not displaying in dropdown
- **Cause:** `categories` prop not passed or empty
- **Solution:** Ensure `medicalOptionsCategory` is loaded and passed to modal

**Issue:** Options not grouping correctly
- **Cause:** Mismatch between `medicalOptionCategoryId` in options and categories
- **Solution:** Verify category IDs match between the two arrays

**Issue:** Table shows no data
- **Cause:** `selectedCategoryId` not set or category has no options
- **Solution:** Check console for errors, verify data structure

## Dependencies

- React (useState, useEffect)
- ReactDOM (createPortal)
- useLocalCurrencyFormat (custom hook)

## Version History

- **v1.0** (2026-04-20): Initial implementation of category grouping feature
  - Added data transformation logic
  - Implemented category dropdown selector
  - Updated modal to display grouped data
  - Added styling for category selector

## Contact

For questions or issues related to this feature, please contact the development team.
