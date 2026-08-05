using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.User;

namespace UserService.Application.Commands.MakeAction;

public class MakeActionHandler : ICommandHandler<Guid, UnitResult<Error>>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public MakeActionHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UnitResult<Error>> Handle(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return Errors.User.NotFound();
        
        if (user.UserAccess.IsSubscribed)
            return Result.Success<Error>();
        
        var debitBalanceResult = user.UserAccess.DebitBalance(1);
        
        if (debitBalanceResult.IsFailure)
            return debitBalanceResult.Error;

        await _unitOfWork.SaveChanges(cancellationToken);
        
        return Result.Success<Error>();
    }
}