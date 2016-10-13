using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IUserInputListener
{
    void OnUserInput(EInputGroup group, ICommand command);
    void OnUserInputKeyDown(EInputGroup group, ICommand command);
}
