using Microsoft.AspNetCore.SignalR;
using StockProcessor.Repositories.Interfaces;

namespace StockProcessor.Hubs {
    public class StockHub : Hub {
        private readonly IUnitOfWork _unitOfWork;

        public StockHub (IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork ??
                throw new System.ArgumentNullException (nameof (unitOfWork));
        }
    }

}