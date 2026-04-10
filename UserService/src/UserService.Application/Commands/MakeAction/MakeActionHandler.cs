using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.MakeAction;

public class MakeActionHandler : ICommandHandler<Guid, UnitResult<ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public MakeActionHandler(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return (ErrorList)Errors.User.NotFound();
        
        var debitBalanceResult = user.UserAccess.DebitBalance(1);
        
        if (user.UserAccess.IsSubscribed == false && debitBalanceResult.IsFailure)
            return user.UserAccess.IsSubscribed == false ? Errors.User.WrongCredentials() : debitBalanceResult.Error;

        await _unitOfWork.SaveChanges(cancellationToken);
        
        return Result.Success<ErrorList>();
    }
}