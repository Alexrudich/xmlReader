using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlReader
{
    public class sqlQuerry
    {
        public static string sqlRequest = @"IF EXISTS(SELECT * FROM dbo.StepKolCargo WHERE TransportNumber=@trNum) 
                            UPDATE dbo.StepKolCargo 
                            SET FileName = @CurFileName,
                                TransportNumber = @trNum,
                                SMGSNumber = @SMGSnum,
                                SmgsDate = @SMGSdt,
                                DeclarationNumber = @DeclNumb,
                                DeclarationDate = @DeclDate,
                                AccountNumber = @AcNumb,
                                AccountDate = @AcDate,
                                RegistrationNumber = @RegNum,
                                RegistrationDate = @RegDate,
                                TempDislocationNumber = @TempDisNum,
                                TempDislocationDate = @TempDisDate,
                                CargoStationID = @CargoStID
                            WHERE TransportNumber=@trNum

                            ELSE INSERT INTO dbo.StepKolCargo(FileName,TransportNumber,SmgsNumber,SmgsDate,DeclarationNumber,DeclarationDate,AccountNumber,AccountDate,RegistrationNumber,RegistrationDate,TempDislocationNumber,TempDislocationDate,CargoStationID) 
                            VALUES(@CurFileName,@trNum,@SMGSnum,@SMGSdt,@DeclNumb,@DeclDate,@AcNumb,@AcDate,@RegNum,@RegDate,@TempDisNum,@TempDisDate,@CargoStID);";
    }
}
