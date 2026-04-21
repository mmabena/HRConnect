import React from 'react';
import './DynamicGrid.css';

const DynamicGrid = ({
  data,
  columns,
  loading,
  error,
  onRowClick = null,
  currentPage,
  totalPages,
  onPageChange
}) => {

  if (loading) return <div className="grid-loading">Loading ...</div>;
  if (error) return <div className="grid-error">Error loading data</div>;
  if (data.length === 0) return <div className="grid-empty">No data available</div>

  const handlePageChanges = (pageNumber) => {
    if(pageNumber >= 1 && pageNumber <= totalPages) {
        onPageChange(pageNumber);
    }
  };



  const getPageNumbers = () => {
    const pages = [];
    const maxPages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
        startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
    }
    return pages;
  };



  return(
    <div className="dynamic-grid-container">
      <div className="grid-wrapper">
          {/* Grid Table */}
          <table className="grid-table">
              <thead>
              {/* Table Header Grid */}
              <tr className="grid-header-row">
                  {columns.map((col) => (
                      <th className="grid-header-cell">
                          {col.header}
                      </th>
                  ))}
              </tr>
              </thead>

              {/* Table body */}
              <tbody>
              {data.map((row, rowIndex) => (
                  <tr
                      key={row.id || rowIndex}
                      className="grid-row"
                      onClick={() => onRowClick && onRowClick(row)}
                  >
                      {columns.map((col) => (
                          <td
                              key={`${rowIndex}-${col.key}`}
                              className="grid-cell"
                          >
                              {col.render
                                  ? col.render(row[col.key], row)
                                  : row[col.key]}
                          </td>
                      ))}
                  </tr>
              ))}
              </tbody>
          </table>
      </div>

      {/* Grid - Pagination */}
      <div className="grid-pagination">
        <button
          className="pagination-btn pagination-btn-first"
          onClick={() => handlePageChanges(1)}
          disabled={currentPage === 1}
        >
          First
        </button>

        <button
          className="pagination-btn pagination-btn-prev"
          onClick={() => handlePageChanges(currentPage - 1)}
        >
          Previous
        </button>

        <div
          className="pagination-number">
          {getPageNumbers().map((page) => (
            <button
              key={page}
              className={`pagination-number ${currentPage === page ? 'active' : ''}`}
              onClick={() => handlePageChanges(page)}
            >
                {page}
            </button>
          ))}
        </div>

        <button
          className="pagination-btn pagination-btn-next"
          onClick={() => handlePageChanges(currentPage + 1)}
          disabled={currentPage === totalPages}
        >
          Next
        </button>

        <button
          className="pagination-btn pagination-btn-last"
          onClick={() => handlePageChanges(totalPages)}
          disabled={currentPage === totalPages}
        >
          Last
        </button>

        <div className="pagination-info">
          <span>Page {currentPage} of {totalPages}</span>
        </div>

      </div>
    </div>
  );
};

export default DynamicGrid;